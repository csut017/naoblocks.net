import logging

class BlockDefinition(object):
    def __init__(self, id, name, image = None, code = None) -> None:
        self.id = id
        self.name = name
        self.image = image
        self.code = code

class ClassLogger(logging.LoggerAdapter):
    def __init__(self, logger: logging.Logger, source) -> None:
        super().__init__(logger, {})
        self._class = source

    def process(self, msg, kwargs):
        return '[{}] {}'.format(self._class, msg), kwargs    

class CodeBuilder(object):

    def __init__(self, template = None) -> None:
        self._template = template if not template is None else 'start{{\n{}\n}}\ngo()'

    def build(self, blocks: list[BlockDefinition], include_ids=False):
        format = '[{0:05}]' if include_ids else ''
        inner = '\n'.join(format.format(int) + b.definition.code for int, b in enumerate(blocks) if not b.definition is None)
        return self._template.format(inner)