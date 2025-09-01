import requests

r = requests.post(
    'http://127.0.0.1:5000/api/v1/passthrough/test/start')
r.raise_for_status()
