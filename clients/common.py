import logging

class ClassLogger(logging.LoggerAdapter):
    def __init__(self, logger: logging.Logger, source) -> None:
        super().__init__(logger, {})
        self._class = source

    def process(self, msg, kwargs):
        return '[{}] {}'.format(self._class, msg), kwargs    
