import math
from unittest import TestCase

from scoring import *


class Test(TestCase):
    def test_musician_qualities(self):
        instruments = np.array([1, 2, 1, 2, 2, 3])
        places = np.array([[0, 0], [0, 1], [0, 2], [1, 0], [1, 2], [1, 3]])
        qualities = musician_qualities(instruments, places)
        sqrt2 = 1 / math.sqrt(2)
        expected = [1 + .5, 1 + sqrt2 + sqrt2, 1 + .5, 1 + .5 + sqrt2, 1 + .5 + sqrt2, 1]
        assert np.array_equal(qualities, expected)
