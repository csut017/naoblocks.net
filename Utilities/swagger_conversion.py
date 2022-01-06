# A utility to convert a Swagger JSON file to CSV
# The CSV file can then be used to configure the security tests
# In future, this script could be converted to a .Net tool that would automatically generate the code

import csv
import json

swagger_path = './Utilities/swagger.json'     # Assuming the Swagger file is in the same directory
csv_path = './Utilities/paths.csv'            # Again, assuming same output directory
with open(swagger_path) as json_file:
    data = json.load(json_file)

with open(csv_path, 'w', newline='') as file:
    writer = csv.writer(file)
    writer.writerow(['Path', 'Verb'])

    for path in data['paths']:
        details = data['paths'][path]
        for verb in details:
            writer.writerow([path, verb])