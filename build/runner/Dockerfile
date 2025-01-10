ARG RUNNER_VERSION=latest
FROM code.forgejo.org/forgejo/runner:${RUNNER_VERSION}

USER root
COPY --from=certs ca.crt  /tmp/ca.crt
RUN cat /tmp/ca.crt >> /etc/ssl/certs/ca-certificates.crt
USER 1000:1000
