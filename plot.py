#!/usr/bin/python3

import numpy as np
import matplotlib.pyplot as plt

def simple_score(placements, attendee, tastes, musicians):
    dxs = attendee[0] - placements[:,0]
    dys = attendee[1] - placements[:,1]
    ds = dxs**2 + dys**2
    return (tastes[musicians] / ds).sum()

placements = np.array([[-1.0, 0], [1.0, 0]])
tastes = np.array([1.0, 1.0])
musicians = np.array([0, 1])

ts = np.linspace(-2.0, 2.0, num=20)
data2d = [[simple_score(placements, np.array([x,y]), tastes, musicians) for x in ts] for y in ts]
data2d = np.array(data2d)

fig, ax = plt.subplots()
im = ax.imshow(data2d)

fig.colorbar(im, ax=ax, label='Interactive colorbar')

plt.show()


