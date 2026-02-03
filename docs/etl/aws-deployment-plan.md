# AWS ETL Deployment Plan (Skeleton)

## Overview
This plan outlines the recommended AWS scheduler and container runtime for the ETL job runner without provisioning any infrastructure yet.

## Recommended Scheduling Topology
- **Ingestion schedule**: EventBridge schedule -> ECS Fargate task (ETL ingestion worker).
- **Queue schedule**: EventBridge schedule -> ECS Fargate task (queue-only worker).

Each task should run independently to avoid contention and to allow separate scaling or throttling when needed.

## IAM Policy Placeholders
Attach IAM policies to the ECS task role that allow the following:
- **CloudWatch Logs**: write logs for task stdout/stderr.
- **Secrets Manager**: read-only access to ETL secrets (connection strings, API keys).
- **S3** (if/when enabled): read/write access for raw payloads or photo storage.

## Configuration Strategy
- Store **connection strings and API keys in environment variables** (or Secrets Manager mapped into environment variables).
- App settings remain templated in repo (`appsettings.*.json`) with placeholders only.
- Use `ASPNETCORE_ENVIRONMENT=Aws` to load `appsettings.Aws.json` alongside `appsettings.json`.

## Deployment Phases
1. **Local**
   - Use `appsettings.Development.json` with local SQL Server Express and user secrets.
2. **AWS Dev**
   - Build and push Docker image; run on ECS Fargate with a dev task definition.
   - Configure secrets + environment variables in the task definition.
3. **AWS Prod**
   - Promote image and task definition with production secrets.
   - Apply tighter IAM scopes and logging retention.

## Entry Commands
Use the following arguments in the ECS task command overrides:
- **Ingestion task**: `--runOnce`
- **Queue task**: `--queue-only --queue-once`
