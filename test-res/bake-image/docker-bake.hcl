target "default" {
  context = "./build"
  platforms = [
    "linux/amd64",
    "linux/arm64",
    "linux/arm/v7",
  ]
}
