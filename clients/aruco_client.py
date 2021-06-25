import cv2
import cv2.aruco as aruco
import logging

from common import ClassLogger

logger = logging.getLogger(__name__)

class BlockDefinition(object):
    def __init__(self, image, code) -> None:
        self.image = image
        self.code = code

class ScannedBlock(object):
    def __init__(self, corners, id) -> None:
        self.corners = corners[0]
        self.id = id[0]
        maxX = max(point[0] for point in self.corners)
        minX = min(point[0] for point in self.corners)
        maxY = max(point[1] for point in self.corners)
        minY = min(point[1] for point in self.corners)
        self.middle = (minX + (maxX - minX) / 2, minY + (maxY - minY) / 2)
        self.definition = None

    def __str__(self) -> str:
        return '{0}: {1}'.format(self.id, self.middle)

class Scanner(object):

    def __init__(self) -> None:
        self._logger = ClassLogger(logger, 'Scanner')
        self.cancel = False
        self._dictionary = {}

    def set_dictionary(self, dictionary):
        self._dictionary = dictionary

    def run(self, camera = 0, show_window = False, dictionary = None) -> None:
        if not dictionary is None:
            self._dictionary = dictionary

        self.cancel = False
        self._logger.info('Starting camera')
        cap = cv2.VideoCapture(camera)
        self._logger.info('Camera started')

        keyPressed = -1
        while not self.cancel and keyPressed == -1:
            _, img = cap.read()
            blocks = self._find_markers(img)
            if show_window:
                cv2.imshow("Image", img)
                keyPressed = cv2.waitKey(50)

        if show_window:
            cv2.destroyAllWindows()

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
                    block.definition = self._dictionary[block.id]
                except KeyError:
                    self._logger.info('Unable to find definition for id {}'.format(block.id))
                result.append(block)

        aruco.drawDetectedMarkers(img, boxes)
        return result

