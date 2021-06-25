import cv2
import cv2.aruco as aruco
import logging
import threading

from common import ClassLogger
from rx.subject import Subject

logger = logging.getLogger(__name__)

class BlockDefinition(object):
    def __init__(self, id, name, image = None, code = None) -> None:
        self.id = id
        self.name = name
        self.image = image
        self.code = code

class ScannedBlock(object):
    def __init__(self, corners, id) -> None:
        self.corners = corners[0]
        self.aruco_id = id[0]
        maxX = max(point[0] for point in self.corners)
        minX = min(point[0] for point in self.corners)
        maxY = max(point[1] for point in self.corners)
        minY = min(point[1] for point in self.corners)
        self.middle = (minX + (maxX - minX) / 2, minY + (maxY - minY) / 2)
        self.definition = None

    def __str__(self) -> str:
        return '{0}: {1}'.format(self.aruco_id, self.middle)

class Scanner(object):

    def __init__(self) -> None:
        self._logger = ClassLogger(logger, 'Scanner')
        self._is_stopping = False
        self._dictionary = {}
        self._thread = None
        self._stopping_lock = threading.Lock()

    def build_dictionary(self, *definitions):
        dictionary = {}
        for definition in definitions:
            dictionary[definition.id] = definition
        self.set_dictionary(dictionary)
        return dictionary

    def stop(self):
        with self._stopping_lock:
            self._is_stopping = True

    def run(self, camera = 0, show_window = False, dictionary = None):
        if not dictionary is None:
            self._dictionary = dictionary

        subject = Subject()
        self._is_stopping = False
        self._thread = threading.Thread(target=self._run_internal, daemon=True, args=(camera, show_window, subject))
        self._thread.start()
        return subject

    def set_dictionary(self, dictionary) -> None:
        self._dictionary = dictionary

    def wait(self) -> None:
        self._thread.join()

    def _run_internal(self, camera, show_window, subject: Subject):
        self._logger.info('Starting camera')
        cap = cv2.VideoCapture(camera)
        self._logger.info('Camera started')

        keyPressed = -1
        cancel = False
        last_program = ''
        while not cancel and keyPressed == -1:
            _, img = cap.read()
            blocks = self._find_markers(img)
            if show_window:
                cv2.imshow("Image", img)
                keyPressed = cv2.waitKey(50)

            program = ':'.join((str(b.definition.id) for b in blocks if not b.definition is None))
            if last_program != program:
                subject.on_next(blocks)

            with self._stopping_lock:
                cancel = self._is_stopping

        if show_window:
            cv2.destroyAllWindows()
        subject.on_completed()

    def _find_markers(self, img):
        img_gray = cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)
        dict = aruco.getPredefinedDictionary(aruco.DICT_6X6_250)
        params = aruco.DetectorParameters_create()
        boxes, ids, _ = aruco.detectMarkers(img_gray, dict, parameters=params)
        result = []
        if not ids is None:
            blocks = (ScannedBlock(item[0], item[1]) for item in zip(boxes, ids))
            for block in blocks:
                try:
                    block.definition = self._dictionary[block.aruco_id]
                except KeyError:
                    self._logger.info('Unable to find definition for id {}'.format(block.id))
                result.append(block)

        aruco.drawDetectedMarkers(img, boxes)
        return result

