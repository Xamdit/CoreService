#!/bin/bash

curl 'https://api-uat.digidoc.asia/login' \
  -H 'accept: */*' \
  -H 'accept-language: en-US,en;q=0.9,th;q=0.8' \
  -H 'content-type: application/x-www-form-urlencoded; charset=UTF-8' \
  -H 'origin: https://localhost' \
  -H 'priority: u=1, i' \
  -H 'referer: https://localhost/' \
  -H 'sec-ch-ua: "Microsoft Edge";v="131", "Chromium";v="131", "Not_A Brand";v="24"' \
  -H 'sec-ch-ua-mobile: ?0' \
  -H 'sec-ch-ua-platform: "macOS"' \
  -H 'sec-fetch-dest: empty' \
  -H 'sec-fetch-mode: cors' \
  -H 'sec-fetch-site: cross-site' \
  -H 'user-agent: Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36 Edg/131.0.0.0' \
  --data-raw 'email=admin%40localhost.com&password=5f4dcc3b5aa765d61d8327deb882cf99'
