import requests

r = requests.post(
    'http://127.0.0.1:5000/api/v1/passthrough/test/ACACACAC')
r.raise_for_status()

r = requests.post(
    'http://127.0.0.1:5000/api/v1/passthrough/test',
    json = {
        'commands': ['A', 'C', 'B', 'D'],
        'count': 4
    })
r.raise_for_status()
