class Sensor(object):
    ''' Encapsulates a sensor. '''

    HEAD_FRONT = 'HEAD_FRONT'
    HEAD_MIDDLE = 'HEAD_MIDDLE'
    HEAD_REAR = 'HEAD_REAR'
    SONAR_LEFT = 'SONAR_LEFT'
    SONAR_RIGHT = 'SONAR_RIGHT'
    BATTERY = 'BATTERY'
    GYROSCOPE_X = 'GYROSCOPE_X'
    GYROSCOPE_Y = 'GYROSCOPE_Y'
    ACCELEROMETER_X = 'ACCELEROMETER_X'
    ACCELEROMETER_Y = 'ACCELEROMETER_Y'
    ACCELEROMETER_Z = 'ACCELEROMETER_Z'
    FOOT_LEFT = 'FOOT_LEFT'
    FOOT_RIGHT = 'FOOT_RIGHT'

    def __init__(self, name, service):
        self.name = name
        self._service = service
        self._log = self._doLog

    def _doLog(self, message):
        print message

    def read(self):
        ''' Reads the sensor value. '''
        value = self._service.getData(self.name)
        self._log('Retrieved ' + str(value) + ' from ' + self.name)
        return self.format(value)

    def format(self, value):
        ''' Formats the value. '''
        return value


class BooleanSensor(Sensor):
    ''' Encapsulates a boolean sensor. '''

    def __init__(self, name, service):
        super(BooleanSensor, self).__init__(name, service)

    def format(self, value):
        ''' Formats the value. '''
        return str(value) == '1'

class FootSensor(Sensor):
    ''' Encapsulates the sensors for the bumpers on a foot. '''

    def __init__(self, name, service):
        super(FootSensor, self).__init__(name, service)

    def read(self):
        ''' Reads the sensors for the foor bumpers. '''
        value = (self._service.getData('Device/SubDeviceList/' + self.name + '/Bumper/Left/Sensor/Value') +
                self._service.getData('Device/SubDeviceList/' + self.name + '/Bumper/Right/Sensor/Value')) >= 1
        self._log('Retrieved ' + str(value) + ' for ' + self.name)
        return value
