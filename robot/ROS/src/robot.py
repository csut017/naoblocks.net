''' Wrappers for working with the Nao robot. '''

import rospy
import time
from sound_play.libsoundplay import SoundClient

from PIL import Image
import logger

class Robot(object):
    ''' Defines a common interface to a ROS-based robot.

NOTE: Currently the interface is based on a Nao robot. We will need to consolidate and move
to a common interface for other robot types.

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
        logger.log('[Robot] Starting ROS robot interface')
        try:
            self.name = rospy.get_param('/robot/name')
        except KeyError:
            self.name = 'unknown'

        self._soundhandle = SoundClient()
        rospy.sleep(1)

    def _log(self, msg):
        logger.log('[Robot:' + self.name + '] ' + msg)

    def _parseLed(self, ledSets, led):
        if led == Robot.RIGHT_EYE:
            ledSets.append('RightFaceLeds')
        elif led == Robot.LEFT_EYE:
            ledSets.append('LeftFaceLeds')
        elif led == Robot.CHEST:
            ledSets.append('ChestLeds')
        else:
            raise InvalidOperationError('Unknown LED: ' + str(led))

    def __enter__(self):
        return self

    def __exit__(self, exit_type, exit_value, exit_traceback):
        pass

    def delay(self, duration):
        ''' Delays for the specified duration. '''
        self._log('Delaying for ' + str(duration) + 's')
        time.sleep(duration)
        return self

    def getJointAngles(self):
        ''' Retrieves the current joint angles of the robot. '''
        self._log('Functionality not implemented: getJointAngles()')
        out = {}
        return out

    def getLastResult(self):
        ''' Returns the result of the last action. '''
        return self._last_result

    def getNotifications(self):
        ''' Returns a list of all the current notifications on the robot. '''
        self._log('Functionality not implemented: getNotifications()')
        notifications = []
        return notifications

    def getPosture(self):
        ''' Retrieves the current posture family. '''
        self._log('Functionality not implemented: getPosture()')
        return ''

    def getSensor(self, name):
        ''' Retrieves a sensor. '''
        self._log('Functionality not implemented: getSensor()')
        return None

    def goToPosture(self, posture, wait=False):
        ''' Starts the robot moving to a posture. '''
        self._log('Functionality not implemented: goToPosture()')
        return self.wait() if wait else self

    def moveHands(self, hands, openHand, wait=False):
        self._log('Functionality not implemented: moveHands()')
        return self.wait() if wait else self

    def muteAudioVolume(self):
        self._log('Functionality not implemented: muteAudioVolume()')
        return self

    def performMovements(self, movements, wait=False):
        self._log('Functionality not implemented: performMovements()')
        return self.wait() if wait else self

    def rest(self):
        self._log('Resting')
        self._log('Functionality not implemented: rest()')
        return self

    def restoreAudioVolume(self):
        self._log('Functionality not implemented: restoreAudioVolume()')

    def say(self, text, wait=False):
        text = str(text)
        self._log('Saying ' + text)

        voice = 'voice_kal_diphone'
        volume = 1.0

        self._soundhandle.say(text, voice, volume)
        rospy.sleep(1)

        return self.wait() if wait else self

    def setAudioVolume(self, volume):
        self._log('Functionality not implemented: setAudioVolume()')

    def setAnimations(self, on):
        self._log('Functionality not implemented: setAnimations()')
        return self

    def setBreathing(self, on):
        self._log('Functionality not implemented: setBreathing()')
        return self

    def setLEDColour(self, leds, colour, fade=None):
        self._log('Functionality not implemented: setLEDColour()')
        return self

    def setSonars(self, on):
        self._log('Functionality not implemented: setSonars()')

    def startBehaviour(self, behaviour, wait=False, skipCheck=False):
        self._log('Functionality not implemented: startBehaviour()')
        return self.wait() if wait else self

    def startVideo(self):
        self._log('Functionality not implemented: startVideo()')
        return self._video

    def wait(self):
        self._log('Waiting')
        return self

    def wakeUp(self):
        self._log('Functionality not implemented: wakeUp()')
        return self

    def walkStart(self, x, y, theta, wait=False, useArms=True):
        self._log('Functionality not implemented: walkStart()')
        return self.wait() if wait else self

    def walkStop(self):
        self._log('Functionality not implemented: walkStop()')
        return self

    def walkTo(self, x, y, theta, wait=False, useArms=True):
        self._log('Functionality not implemented: walkTo()')
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
