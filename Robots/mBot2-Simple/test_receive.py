import requests

r = requests.get('http://127.0.0.1:5000/api/v1/passthrough/status')
r.raise_for_status()
print(r.json())

r = requests.get('http://127.0.0.1:5000/api/v1/passthrough/test')
r.raise_for_status()
print(r.text)
