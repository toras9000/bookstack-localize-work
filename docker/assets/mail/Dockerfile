FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /work
COPY ./recv-mail.cs1  ./

RUN dotnet publish \
        --configuration Release \
        --output /work/publish \
        recv-mail.cs1


FROM mcr.microsoft.com/dotnet/runtime:10.0

WORKDIR /app
COPY --chmod=+x --from=builder  /work/publish  /app

CMD ["/app/recv-mail"]
