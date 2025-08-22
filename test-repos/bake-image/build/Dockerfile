FROM alpine

RUN apk add --no-cache git curl
RUN curl http://myserver.home/certs/ca.crt >> /etc/ssl/certs/ca-certificates.crt

WORKDIR /work
RUN --mount=type=secret,id=access-token,env=ACCESS_TOKEN git clone https://dummyname:${ACCESS_TOKEN}@forgejo.myserver.home/toras9000/bake-image.git

CMD ["cat", "/work/bake-image/docker-bake.hcl"]
