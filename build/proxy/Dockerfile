ARG NGINX_VERSION=version-tag
FROM nginx:${NGINX_VERSION}

RUN mkdir /etc/nginx/conf_extra.d                                           \
 && echo ""                                        >> /etc/nginx/nginx.conf \
 && echo "include /etc/nginx/conf_extra.d/*.conf;" >> /etc/nginx/nginx.conf

