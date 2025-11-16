terraform {
  backend "s3" {
    bucket       = "terraform-state-fo-gepuonlcjhv"
    key          = "terraform.tfstate"
    region       = "eu-central-1"
    use_lockfile = true
    encrypt      = true
  }
}
