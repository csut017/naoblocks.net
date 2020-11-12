''' Wrappers for working with the Nao robot. '''

import math
import time

import qi
from PIL import Image
import sensors
import logger

class Robot(object):
    ''' Defines a common interface to a Nao robot.

Most methods are either async or happen quickly. If the caller needs to wait for an action to 
finish, call either the wait() method or use the wait argument on most methods (this will 
internally call the wait() method after starting the requested action. Because of the way this 
has been implemented, the result of an action needs to be retrieved by calling lastResult().

Most methods are designed to be chained (i.e. they return self.) This allows for methods calls 
to be written in a single line. For example:

r.say("Hello world").goToPosture(Robot.STAND).wait() '''

    SIT = 'Sit'
    STAND = 'Stand'
    CROUCH = 'Crouch'

    LIEONBACK = 'LyingBack'
    LIEONBELLY = 'LyingBelly'
    SITBACK = 'SitRelax'
    SITFORWARD = 'Sit'
    STANDINIT = 'StandInit'
    STANDZERO = 'StandZero'

    RIGHT_EYE = 1
    LEFT_EYE = 2
    CHEST = 4

    RIGHT_HAND = 1
    LEFT_HAND = 2

    def __init__(self, ip):
        self._ip = ip
        self._session = qi.Session()
        address = "tcp://" + ip + ":9559"
        logger.log('[Robot] Connecting to ' + address)
        self._session.connect(address)
        self._audio = self._session.service("ALAudioDevice")
        self._behavior = self._session.service("ALBehaviorManager")
        self._leds = self._session.service("ALLeds")
        self._memory = self._session.service("ALMemory")
        self._motion = self._session.service("ALMotion")
        self._notifications = self._session.service("ALNotificationManager")
        self._posture = self._session.service("ALRobotPosture")
        self._sonar = self._session.service("ALSonar")
        self._speech = self._session.service("ALAnimatedSpeech")
        self._system = self._session.service("ALSystem")
        self._video = None
        self._promises = []
        self._last_result = False
        self._resting = True
        self._last_volume = self._audio.getOutputVolume()
        self.name = self._system.robotName()
        self._sensors = {
            sensors.Sensor.HEAD_FRONT: sensors.BooleanSensor('Device/SubDeviceList/Head/Touch/Front/Sensor/Value', self._memory),
            sensors.Sensor.HEAD_MIDDLE: sensors.BooleanSensor('Device/SubDeviceList/Head/Touch/Middle/Sensor/Value', self._memory),
            sensors.Sensor.HEAD_REAR: sensors.BooleanSensor('Device/SubDeviceList/Head/Touch/Rear/Sensor/Value', self._memory),
            sensors.Sensor.SONAR_LEFT: sensors.Sensor('Device/SubDeviceList/US/Left/Sensor/Value', self._memory),
            sensors.Sensor.SONAR_RIGHT: sensors.Sensor('Device/SubDeviceList/US/Right/Sensor/Value', self._memory),
            sensors.Sensor.BATTERY: sensors.Sensor('Device/SubDeviceList/Battery/Current/Sensor/Value', self._memory),
            sensors.Sensor.GYROSCOPE_X: sensors.Sensor('Device/SubDeviceList/InertialSensor/GyroscopeX/Sensor/Value', self._memory),
            sensors.Sensor.GYROSCOPE_Y: sensors.Sensor('Device/SubDeviceList/InertialSensor/GyroscopeY/Sensor/Value', self._memory),
            sensors.Sensor.ACCELEROMETER_X: sensors.Sensor('Device/SubDeviceList/InertialSensor/AccX/Sensor/Value', self._memory),
            sensors.Sensor.ACCELEROMETER_Y: sensors.Sensor('Device/SubDeviceList/InertialSensor/AccY/Sensor/Value', self._memory),
            sensors.Sensor.ACCELEROMETER_Z: sensors.Sensor('Device/SubDeviceList/InertialSensor/AccZ/Sensor/Value', self._memory),
            sensors.Sensor.FOOT_LEFT: sensors.FootSensor('LFoot', self._memory),
            sensors.Sensor.FOOT_RIGHT: sensors.FootSensor('RFoot', self._memory)
        }
        self._sonars_on = False

    def _log(self, msg):
        logger.log('[Robot:' + self.name + '] ' + msg)

    def _moveHand(self, hand, openHand):
        handName = None
        if hand == Robot.RIGHT_HAND:
            handName = 'RHand'
        elif hand == Robot.LEFT_HAND:
            handName = 'LHand'
        else:
            raise InvalidOperationError(
                'moveHand', 'Unknown hand: ' + str(hand))

        self._log('' + ('Opening ' if openHand else 'Closing ') + handName)
        if openHand:
            p = self._motion.openHand(handName, _async=True)
        else:
            p = self._motion.closeHand(handName, _async=True)
        self._promises.append(p)

    def _parseLed(self, ledSets, led):
        if led == Robot.RIGHT_EYE:
            ledSets.append('RightFaceLeds')
        elif led == Robot.LEFT_EYE:
            ledSets.append('LeftFaceLeds')
        elif led == Robot.CHEST:
            ledSets.append('ChestLeds')
        else:
            raise InvalidOperationError('Unknown LED: ' + str(led))

    def _prepare(self):
        if self._resting:
            self.wakeUp()

    def __enter__(self):
        return self

    def __exit__(self, exit_type, exit_value, exit_traceback):
        self.rest()
        self.setSonars(False)

    def delay(self, duration):
        ''' Delays for the specified duration. '''
        self._log('Delaying for ' + str(duration) + 's')
        time.sleep(duration)
        return self

    def getJointAngles(self):
        ''' Retrieves the current joint angles of the robot. '''
        joints = ['HeadYaw', 'HeadPitch', 'LShoulderPitch', 'LShoulderRoll', 'LElbowYaw', 'LElbowRoll',
                  'LWristYaw', 'LHand', 'LHipYawPitch', 'LHipRoll', 'LHipPitch', 'LKneePitch',
                  'LAnklePitch', 'RAnkleRoll', 'RHipYawPitch', 'RHipRoll', 'RHipPitch', 'RKneePitch',
                  'RAnklePitch', 'LAnkleRoll', 'RShoulderPitch', 'RShoulderRoll', 'RElbowYaw',
                  'RElbowRoll', 'RWristYaw', 'RHand']
        angles = self._motion.getAngles(joints, True)
        out = {}
        for pos in range(len(joints)):
            out[joints[pos]] = angles[pos]
        return out

    def getLastResult(self):
        ''' Returns the result of the last action. '''
        return self._last_result

    def getNotifications(self):
        ''' Returns a list of all the current notifications on the robot. '''
        notifications = self._notifications.notifications()
        return [Notification(n, self) for n in notifications]

    def getNotification(self, id):
        ''' Returns a notification on the robot. '''
        self._log('Retrieving notification %d' % (id, ))
        notification = self._notifications.notification(id)
        return Notification(notification, self)

    def addNotification(self, message, severity='info'):
        self._log('Adding notification "%s" [%s]' % (message, severity))
        self._notifications.add({"message": message, "severity": severity, "removeOnRead": True})

    def getPosture(self):
        ''' Retrieves the current posture family. '''
        return self._posture.getPostureFamily()

    def getSensor(self, name):
        ''' Retrieves a sensor. '''
        sensor = self._sensors[name]
        sensor._log = self._log
        return sensor

    def goToPosture(self, posture, wait=False):
        ''' Starts the robot moving to a posture. '''
        self._prepare()
        self._log('Going to posture ' + posture)
        p = self._posture.goToPosture(posture, 0.8, _async=True)
        self._promises.append(p)
        return self.wait() if wait else self

    def moveHands(self, hands, openHand, wait=False):
        ''' Moves the hands

This method can move either one or both hands. The possible movements are either open or close. '''
        try:
            for hand in hands:
                self._moveHand(hand, openHand)
        except TypeError:
            self._moveHand(hand, openHand)
        return self.wait() if wait else self

    def muteAudioVolume(self):
        volume = self._audio.getOutputVolume()
        if volume > 0.0:
            self._last_volume = volume
            self._audio.setOutputVolume(0)
        return self

    def performMovements(self, movements, wait=False):
        self._prepare()
        names = []
        angles = []
        times = []
        try:
            _ = iter(movements)
        except TypeError:
            movements = [movements]

        for movement in movements:
            names += movement.names()
            angles += movement.angles()
            times += movement.times()

        self._log('Performing movement(s)')
        p = self._motion.angleInterpolation(
            names, angles, times, True, _async=True)
        self._promises.append(p)
        return self.wait() if wait else self

    def rest(self):
        self._log('Resting')
        self._motion.rest()
        self._resting = True
        return self

    def restoreAudioVolume(self):
        self._audio.setOutputVolume(self._last_volume)

    def say(self, text, wait=False):
        text = str(text)
        self._log('Saying ' + text)
        p = self._speech.say(text, _async=True)
        self._promises.append(p)
        return self.wait() if wait else self

    def setAudioVolume(self, volume):
        self._audio.setOutputVolume(volume)

    def setAnimations(self, on):
        ''' Turns animations (body language) on or off.

This method can take in either a boolean (True/False), or an integer between 0 and 2.
If the argument is True, then the animations are set to contextual (2), if False then they are set to none (0)'''
        if on == True or on == False:
            self._log('Animations are now ' + ('on' if on else 'off'))
            self._speech.setBodyLanguageMode(2 if on else 0)
        else:
            self._log('Setting animations to ' + str(on))
            self._speech.setBodyLanguageMode(on)
        return self

    def setBreathing(self, on):
        ''' Turns breathing on or off. '''
        self._log('Breathing is now ' + ('on' if on else 'off'))
        self._motion.setBreathEnabled('Body', on)
        return self

    def setLEDColour(self, leds, colour, fade=None):
        ledSets = []

        try:
            for led in leds:
                self._parseLed(ledSets, led)
        except TypeError:
            self._parseLed(ledSets, leds)

        colours = ['Red', 'Green', 'Blue']
        rgb = _rgb(colour)
        self._log('Changing LED colour(s)')
        for ledSet in ledSets:
            for idx, colour_name in enumerate(colours):
                if rgb[idx] >= 128:
                    if fade is None:
                        self._leds.on(ledSet + colour_name, _async=True)
                    else:
                        self._leds.fade(ledSet + colour_name,
                                        1, fade, _async=True)
                else:
                    if fade is None:
                        self._leds.off(ledSet + colour_name, _async=True)
                    else:
                        self._leds.fade(ledSet + colour_name,
                                        0, fade, _async=True)
        return self

    def setSonars(self, on):
        if on:
            if not self._sonars_on:
                self._log('Turning sonars on')
                self._sonar.subscribe("NaoRobotAPI")
                self._sonars_on = True
        else:
            if self._sonars_on:
                self._log('Turning sonars off')
                self._sonar.unsubscribe("NaoRobotAPI")
                self._sonars_on = False

    def startBehaviour(self, behaviour, wait=False, skipCheck=False):
        startRun = False
        if skipCheck:
            startRun = True
        else:
            self._log('Checking for behaviour "' + behaviour + '"')
            if (self._behavior.isBehaviorInstalled(behaviour)):
                if (not self._behavior.isBehaviorRunning(behaviour)):
                    startRun = True
                else:
                    raise InvalidOperationError(
                        'startBehaviour', 'Behaviour already running')
            else:
                raise InvalidOperationError(
                    'startBehaviour', 'Behaviour not found')

        if startRun:
            self._log('Starting behaviour "' + behaviour + '"')
            p = self._behavior.runBehavior(behaviour, _async=True)
            self._promises.append(p)

        return self.wait() if wait else self

    def startVideo(self):
        if self._video is None:
            self._video = VideoCamera(self)

        return self._video

    def wait(self):
        self._log('Waiting')
        for p in self._promises:
            self._last_result = p.value()
        self._promises = []
        return self

    def wakeUp(self):
        self._log('Waking up')
        self._motion.wakeUp()
        self._resting = False
        return self

    def walkStart(self, x, y, theta, wait=False, useArms=True):
        ''' Starts the robot walking in a direction. '''
        x_direction = float(x)
        y_direction = float(y)
        MAX_SPEED = 1.0
        if x_direction > MAX_SPEED:
            x_direction = MAX_SPEED
        if x_direction < -MAX_SPEED:
            x_direction = -MAX_SPEED
        if y_direction > MAX_SPEED:
            y_direction = MAX_SPEED
        if y_direction < -MAX_SPEED:
            y_direction = -MAX_SPEED
        if theta > 0.5:
            theta = 0.5
        if theta < -0.5:
            theta = -0.5
        self._prepare()
        self._log('Starting walk (%f, %f, %f)' %
                  (x_direction, y_direction, theta))
        posture = self._posture.getPostureFamily()
        if posture != 'Standing':
            raise InvalidOperationError('walkStart', 'Robot must be standing')
        self._motion.setMoveArmsEnabled(useArms, useArms)
        p = self._motion.moveToward(x, y, math.radians(theta), _async=True)
        self._promises.append(p)
        return self.wait() if wait else self

    def walkStop(self):
        ''' Stops the robot from walking. '''
        self._log('Stopping walk')
        self._motion.stopMove()
        return self

    def walkTo(self, x, y, theta, wait=False, useArms=True):
        self._prepare()
        self._log('Walking')
        posture = self._posture.getPostureFamily()
        if posture != 'Standing':
            raise InvalidOperationError('walkTo', 'Robot must be standing')
        self._motion.setMoveArmsEnabled(useArms, useArms)
        p = self._motion.moveTo(x, y, math.radians(theta), _async=True)
        self._promises.append(p)
        return self.wait() if wait else self


class Animations(object):
    ''' Defines some common animations.

These replace the 'magic' strings that would be otherwise needed. '''

    DISABLED = '^mode(disabled)'
    CONTEXTUAL = '^mode(contextual)'


class InvalidOperationError(Exception):
    ''' An invalid operation has been requested. '''

    def __init__(self, operation, message=None):
        self._message = message
        self._operation = operation

    def __str__(self):
        return 'Cannot perform ' + self._operation + ('' if self._message is None else ': ' + self._message)


_NUMERALS = '0123456789abcdefABCDEF'
_HEXDEC = {v: int(v, 16)
           for v in (x + y for x in _NUMERALS for y in _NUMERALS)}


def _rgb(triplet):
    return _HEXDEC[triplet[1:3]], _HEXDEC[triplet[3:5]], _HEXDEC[triplet[5:7]]


def waitForAll(robots):
    ''' Waits for multiple robots to finish any pending actions. '''
    for robot in robots:
        robot.wait()


class Notification(object):
    ''' Encapsulates the notification info. '''

    def __init__(self, data, robot):
        dataDict = dict(data)
        self._robot = robot
        self.id = dataDict['id']
        self.message = dataDict['message']
        self.severity = dataDict['severity']

    def remove(self):
        self._robot._notifications.remove(self.id)


class VideoCamera(object):
    TOP = 0
    BOTTOM = 1

    def __init__(self, robot):
        self._log = robot._log
        self._closed = False
        self._lastCamera = -1
        self._video = robot._session.service('ALVideoDevice')

        resolution = vision_definitions.kQVGA
        colorSpace = vision_definitions.kRGBColorSpace
        fps = 15

        self._log('Subscribing to video cameras')
        self._nameID = self._video.subscribe(
            'RobotVideo', resolution, colorSpace, fps)

    def __del__(self):
        self.close()

    def close(self):
        if self._closed:
            self._log('Camera already unsubscribed')
            return

        self._log('Unsubscribing from camera')
        self._video.unsubscribe(self._nameID)
        self._closed = True

    def current(self):
        self._lastCamera = self._video.getActiveCamera()
        return self._lastCamera

    def capture(self, camera):
        if self._lastCamera != camera:
            cameraName = 'top' if camera == VideoCamera.TOP else 'bottom'
            self._log('Changing camera to %s' % (cameraName))
            self._video.setActiveCamera(camera)
            self._lastCamera = camera

        self._log('Retrieving image')
        naoImage = self._video.getImageRemote(self._nameID)
        if naoImage is None:
            self._log('Unable to retrieve image')
            return None

        self._log('Converting image')
        imageWidth = naoImage[0]
        imageHeight = naoImage[1]
        array = naoImage[6]
        try:
            pilImage = Image.fromstring(
                'RGB', (imageWidth, imageHeight), bytes(array))
        except:
            pilImage = Image.frombytes(
                'RGB', (imageWidth, imageHeight), bytes(array))
        return pilImage
