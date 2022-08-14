import copy
import math


class Movement(object):

    def names(self):
        raise NotImplementedError('Subclass must implement abstract method')

    def angles(self):
        raise NotImplementedError('Subclass must implement abstract method')

    def times(self):
        raise NotImplementedError('Subclass must implement abstract method')


class ArmMovement(Movement):
    LEFT = 'L'
    RIGHT = 'R'

    def __init__(self, arm, start_time=0.0):
        self._arm = arm
        self._last_time = start_time
        self._names = [arm + j for j in ['ShoulderPitch',
                                         'ShoulderRoll', 'ElbowYaw', 'ElbowRoll', 'WristYaw']]
        self._times = []
        self._angles = [[], [], [], [], []]
        self._last = [0.0, 0.0, 0.0, 0.0, 0.0]

    def _generateAngles(self):
        return copy.copy(self._last)

    def addAngles(self, angles, time_point, as_rads=False):
        self._last_time = self._last_time + time_point
        self._times.append(self._last_time)
        if as_rads:
            for pos in range(len(self._angles)):
                self._angles[pos].append(angles[pos])
        else:
            for pos in range(len(self._angles)):
                self._angles[pos].append(math.radians(angles[pos]))
        self._last = copy.copy(angles)
        return self

    def pause(self, duration):
        return self.addAngles(self._last, duration)

    def pointAhead(self, duration=1.0):
        angles = [0.0, 0.0, 0.0, 0.0, 0.0]
        return self.addAngles(angles, duration)

    def pointDown(self, duration=1.0):
        angles = [1.57086, -0.13964, 0.07206, 0.08595, 1.42811]
        if self._arm == ArmMovement.LEFT:
            angles = [1.56617, 0.18250, -0.15958, -0.11654, -1.41899]
        return self.addAngles(angles, duration, as_rads=True)

    def pointOut(self, duration=1.0):
        angles = [0.34059, -1.29627, 0.029104, 0.03491, 0.03984]
        if self._arm == ArmMovement.LEFT:
            angles = [0.45402, 1.30386, -0.06140, -0.03491, -0.23168]
        return self.addAngles(angles, duration, as_rads=True)

    def pointUp(self, duration=1.0):
        angles = [-1.37135, -0.15651, 0.05978, 0.04760, 1.82387]
        if self._arm == ArmMovement.LEFT:
            angles = [-1.59847, 0.00763, -0.10742, -0.15489, -1.55859]
        return self.addAngles(angles, duration, as_rads=True)

    def names(self):
        return self._names

    def angles(self):
        return self._angles

    def times(self):
        return [self._times for _ in range(5)]


class HeadMovement(Movement):

    def __init__(self, start_time=0.0):
        self._times = []
        self._last_time = start_time
        self._yaw_angles = []
        self._last_yaw = 0
        self._pitch_angles = []
        self._last_pitch = 0

    def names(self):
        return ['HeadYaw', 'HeadPitch']

    def angles(self):
        return [self._yaw_angles, self._pitch_angles]

    def times(self):
        return [self._times, self._times]

    def lookAhead(self, duration=None):
        return self.setYawAndPitch(0, 0, duration)

    def lookDown(self, duration=None):
        return self.setYawAndPitch(self._last_yaw, 29, duration)

    def lookLeft(self, duration=None):
        return self.setYawAndPitch(70, self._last_pitch, duration)

    def lookRight(self, duration=None):
        return self.setYawAndPitch(-70, self._last_pitch, duration)

    def lookUp(self, duration=None):
        return self.setYawAndPitch(self._last_yaw, -38, duration)

    def pause(self, duration):
        return self.setYawAndPitch(self._last_yaw, self._last_pitch, duration)

    def setYawAndPitch(self, yaw, pitch, duration=None):
        if duration is None:
            yaw_duration = (abs(self._last_yaw) + abs(yaw)) / 80
            pitch_duration = (abs(self._last_pitch) + abs(pitch)) / 38
            duration = yaw_duration if yaw_duration > pitch_duration else pitch_duration

        if duration < 1:
            duration = 1

        self._last_time += duration
        self._times.append(self._last_time)
        self._yaw_angles.append(math.radians(yaw))
        self._last_yaw = yaw
        self._pitch_angles.append(math.radians(pitch))
        self._last_pitch = pitch
        return self


class RepeatMovement(Movement):

    def __init__(self, movement, times):
        self._movement = movement
        self._number_of_times = times
        self._names = None
        self._angles = None
        self._times = None

    def names(self):
        if self._names is None:
            self._names = self._movement.names()
        return self._names

    def angles(self):
        if not self._angles is None:
            return self._angles

        angles = self._movement.angles()
        self._angles = copy.deepcopy(angles)
        for _ in range(1, self._number_of_times):
            for pos in range(len(angles)):
                self._angles[pos] += angles[pos]

        return self._angles

    def times(self):
        if not self._times is None:
            return self._times

        times = self._movement.times()
        self._times = copy.deepcopy(times)
        for pos in range(1, len(self._times)):
            for _ in range(1, self._number_of_times):
                last_time = self._times[pos][-1]
                for val in times[pos]:
                    self._times[pos].append(val + last_time)

        return self._times


class BodyMovement(Movement):

    def __init__(self):
        self._names = []
        self._angles = []
        self._times = []

    def names(self):
        return self._names

    def angles(self):
        return self._angles

    def times(self):
        return self._times

    def addJoint(self, joint_name, joint_angles, joint_times):
        if len(joint_angles) != len(joint_times):
            raise ValueError('Angle and time lengths are different')

        self._names.append(joint_name)
        self._angles.append(joint_angles)
        self._times.append(joint_times)

    def wave(self):
        movement = BodyMovement()

        movement.addJoint('HeadPitch',
                           [0.29602, -0.170316, -0.340591, 
                               -0.0598679, -0.193327, -0.01078],
                           [0.8, 1.56, 2.24, 2.8, 3.48, 4.6])

        movement.addJoint('HeadYaw',
                           [-0.135034, -0.351328, -0.415757, 
                               -0.418823, -0.520068, -0.375872],
                           [0.8, 1.56, 2.24, 2.8, 3.48, 4.6])

        movement.addJoint('LElbowRoll',
                           [-1.37902, -1.29005, -1.18267, 
                               -1.24863, -1.3192, -1.18421],
                           [0.72, 1.48, 2.16, 2.72, 3.4, 4.52])

        movement.addJoint('LElbowYaw',
                           [-0.803859, -0.691876, -0.679603, 
                               -0.610574, -0.753235, -0.6704],
                           [0.72, 1.48, 2.16, 2.72, 3.4, 4.52])

        movement.addJoint('LHand',
                           [0.238207, 0.240025],
                           [1.48, 4.52])

        movement.addJoint('LShoulderPitch',
                           [1.11824, 0.928028, 0.9403,
                               0.862065, 0.897349, 0.842125],
                           [0.72, 1.48, 2.16, 2.72, 3.4, 4.52])

        movement.addJoint('LShoulderRoll',
                           [0.363515, 0.226991, 0.20398,
                               0.217786, 0.248467, 0.226991],
                           [0.72, 1.48, 2.16, 2.72, 3.4, 4.52])

        movement.addJoint('LWristYaw',
                           [0.147222, 0.11961],
                           [1.48, 4.52])

        movement.addJoint('RElbowRoll',
                           [1.38524, 0.242414, 0.349066, 0.934249, 0.680678,
                               0.191986, 0.261799, 0.707216, 1.01927, 1.26559],
                           [0.64, 1.4, 1.68, 2.08, 2.4, 2.64, 3.04, 3.32, 3.72, 4.44])

        movement.addJoint('RElbowYaw',
                           [-0.312978, 0.564471, 0.391128, 0.348176,
                               0.381923, 0.977384, 0.826783],
                           [0.64, 1.4, 2.08, 2.64, 3.32, 3.72, 4.44])

        movement.addJoint('RHand',
                           [0.853478, 0.854933, 0.425116],
                           [1.4, 3.32, 4.44])

        movement.addJoint('RShoulderPitch',
                           [0.247016, -1.17193, -1.0891, 
                               -1.26091, -1.14892, 1.02015],
                           [0.64, 1.4, 2.08, 2.64, 3.32, 4.44])

        movement.addJoint('RShoulderRoll',
                           [-0.242414, -0.954191, -0.460242, 
                               -0.960325, -0.328317, -0.250085],
                           [0.64, 1.4, 2.08, 2.64, 3.32, 4.44])

        movement.addJoint('RWristYaw',
                           [-0.312978, -0.303775, 0.182504],
                           [1.4, 3.32, 4.44])

        return movement

    def wipeForehead(self):
        movement = BodyMovement()

        movement.addJoint('HeadPitch', 
            [-0.0261199, 0.427944, 0.308291, 0.11194, -0.013848, 0.061318], 
            [0.96, 1.68, 3.28, 3.96, 4.52, 5.08])

        movement.addJoint('HeadYaw', 
            [-0.234743, -0.622845, -0.113558, -0.00617796, -0.027654, -0.036858], 
            [0.96, 1.68, 3.28, 3.96, 4.52, 5.08])

        movement.addJoint('LElbowRoll', 
            [-0.866668, -0.868202, -0.822183, -0.992455, -0.966378, -0.990923], 
            [0.8, 1.52, 3.12, 3.8, 4.36, 4.92])

        movement.addJoint('LElbowYaw', 
            [-0.957257, -0.823801, -1.00788, -0.925044, -1.24412, -0.960325], 
            [0.8, 1.52, 3.12, 3.8, 4.36, 4.92])

        movement.addJoint('LHand', 
            [0.132026, 0.132026, 0.132026, 0.132026], 
            [1.52, 3.12, 3.8, 4.92])

        movement.addJoint('LShoulderPitch', 
            [0.863599, 0.858999, 0.888144, 0.929562, 1.017, 0.977116],
            [0.8, 1.52, 3.12, 3.8, 4.36, 4.92])

        movement.addJoint('LShoulderRoll',
            [0.286815, 0.230059, 0.202446, 0.406468, 0.360449, 0.31903],
            [0.8, 1.52, 3.12, 3.8, 4.36, 4.92])

        movement.addJoint('LWristYaw',
            [0.386526, 0.386526, 0.386526, 0.386526],
            [1.52, 3.12, 3.8, 4.92])

        movement.addJoint('RElbowRoll',
            [1.28093, 1.39752, 1.57239, 1.24105, 1.22571, 0.840674],
            [0.64, 1.36, 2.96, 3.64, 4.2, 4.76])

        movement.addJoint('RElbowYaw',
            [-0.128898, -0.285367, -0.15651, 0.754686, 1.17193, 0.677985],
            [0.64, 1.36, 2.96, 3.64, 4.2, 4.76])

        movement.addJoint('RHand',
            [0.166571, 0.166208, 0.166571, 0.166208],
            [1.36, 2.96, 3.64, 4.76])

        movement.addJoint('RShoulderPitch',
            [0.0767419, -0.59515, -0.866668, -0.613558, 0.584497, 0.882091],
            [0.64, 1.36, 2.96, 3.64, 4.2, 4.76])

        movement.addJoint('RShoulderRoll',
            [-0.019984, -0.019984, -0.615176, -0.833004, -0.224006, -0.214801],
            [0.64, 1.36, 2.96, 3.64, 4.2, 4.76])

        movement.addJoint('RWristYaw',
            [-0.058334, -0.0521979, -0.067538, -0.038392],
            [1.36, 2.96, 3.64, 4.76])

        return movement

class Dances(object):
    GANGNAM = 'gangnam-fb8eb6/gangnam'
    MACARENA = 'macarena-d73ebc/Macarena'
    TAICHI = 'taichi-7eb148/taichi'

class Movements(object):
    CLAP = 'clap-f1a45d/clap'
    