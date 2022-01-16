import logger

class RobotMock(object):
    ''' Fakes a robot instance. This is used primarily for testing the rest of the components on a PC. '''

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

    def __getattr__(self, name):
        def _missing(*args, **kwargs):
            logger.log('[Fake] Called method "%s"', name)
            return self
        return _missing

    def __enter__(self):
        return self

    def __exit__(self, exit_type, exit_value, exit_traceback):
        pass

    def wait(self):
        logger.log('[Fake] Waiting')
        pass
