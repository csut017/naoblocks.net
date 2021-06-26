import cv2
import cv2.aruco as aruco
import logging
import threading

from common import BlockDefinition, ClassLogger
from rx.subject import Subject

logger = logging.getLogger(__name__)

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

    def build_dictionary(self, *definitions: list[BlockDefinition]):
        dictionary = {}
        for definition in definitions:
            dictionary[definition.id] = definition
        self.set_dictionary(dictionary)
        return dictionary

    def stop(self):
        with self._stopping_lock:
            self._is_stopping = True

    def run(self, camera = 0, show_window = False, show_id = False, dictionary = None):
        if not dictionary is None:
            self._dictionary = dictionary

        subject = Subject()
        self._is_stopping = False
        self._thread = threading.Thread(target=self._run_internal, daemon=True, args=(camera, show_window, show_id, subject))
        self._thread.start()
        return subject

    def set_dictionary(self, dictionary) -> None:
        self._dictionary = dictionary

    def wait(self) -> None:
        self._thread.join()

    def _run_internal(self, camera, show_window, show_id, subject: Subject):
        self._logger.info('Starting camera')
        cap = cv2.VideoCapture(camera)
        self._logger.info('Camera started')

        keyPressed = -1
        cancel = False
        while not cancel and keyPressed == -1:
            _, img = cap.read()
            blocks = self._find_markers(img)
            if show_id:
                for block in blocks:
                    text = str(block.aruco_id)
                    colour = (0, 0, 0)
                    size = cv2.getTextSize(text, cv2.FONT_HERSHEY_PLAIN, 1.0, 1)
                    org = (int(block.middle[0] - size[0][0] / 2), int(block.middle[1] - size[0][1] / 2))
                    cv2.putText(img, text, org, cv2.FONT_HERSHEY_PLAIN, 1.0, colour)
            if show_window:
                cv2.imshow("Image", img)
                keyPressed = cv2.waitKey(50)

            subject.on_next(blocks)
            with self._stopping_lock:
                cancel = self._is_stopping

        if show_window:
            cv2.destroyAllWindows()
        subject.on_completed()

    def _find_markers(self, img) -> list[ScannedBlock]:
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

