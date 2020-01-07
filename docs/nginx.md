# nginx

nginx can be used to configure the proxy between a locally running server instance and a remote client (or robot.)

## Configuration

The following is an example of a configuration file to allow clients to connect:

```Nginx
server {
	listen 80 default_server;
	listen [::]:80 default_server;

	root /var/www/html;
	index index.html index.htm;

	server_name _;

	location / {
		proxy_pass		http://localhost:5000;
		proxy_http_version	1.1;
		proxy_set_header	Upgrade $http_upgrade;
		proxy_set_header	Connection keep-alive;
		proxy_set_header	Host $host;
		proxy_cache_bypass	$http_upgrade;
		proxy_set_header	X-Forwarded-For $proxy_add_x_forwarded_for;
		proxy_set_header	X-Forwarded-Proto $scheme;
	}

	location /api/v1/connections/ {
		proxy_set_header	X-Forwarded-For $proxy_add_x_forwarded_for;
		proxy_set_header	Host $host;
		proxy_pass		http://localhost:5000;
		proxy_http_version	1.1;
		proxy_set_header	Upgrade $http_upgrade;
		proxy_set_header	Connection "upgrade";
	}
}
```