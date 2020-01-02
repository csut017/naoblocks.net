class RobotMock(object):
    ''' Fakes a robot instance. This is used primarily for testing the rest of the components on a PC. '''

    def __getattr__(self, name):
        def _missing(*args, **kwargs):
            print '[Fake] Called method "%s"' % (name)
            return self
        return _missing

    def __enter__(self):
        return self

    def __exit__(self, exit_type, exit_value, exit_traceback):
        pass

    def wait(self):
        print '[Fake] Waiting'
        pass