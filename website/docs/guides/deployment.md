# Price Feed Service Deployment Guide

## Overview

This guide provides comprehensive instructions for deploying the Price Feed Service in production environments. It covers infrastructure setup, configuration, monitoring, and maintenance procedures.

## Prerequisites

- Node.js v18 or higher
- Docker v20 or higher
- Kubernetes v1.24 or higher
- Neo N3 node access
- Access to price data APIs
- Prometheus and Grafana for monitoring

## Infrastructure Setup

### Kubernetes Configuration

Create a Kubernetes deployment configuration in `k8s/price-feed.yaml`:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: price-feed
  namespace: neo-service-layer
spec:
  replicas: 3
  selector:
    matchLabels:
      app: price-feed
  template:
    metadata:
      labels:
        app: price-feed
    spec:
      containers:
      - name: price-feed
        image: neo-service-layer/price-feed:1.2.0
        ports:
        - containerPort: 3000
        env:
        - name: NODE_ENV
          value: "production"
        - name: NEO_NETWORK
          value: "mainnet"
        - name: NEO_RPC_URL
          valueFrom:
            secretKeyRef:
              name: neo-secrets
              key: rpc-url
        resources:
          requests:
            cpu: "500m"
            memory: "512Mi"
          limits:
            cpu: "1000m"
            memory: "1Gi"
        livenessProbe:
          httpGet:
            path: /health
            port: 3000
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /ready
            port: 3000
          initialDelaySeconds: 5
          periodSeconds: 5
        volumeMounts:
        - name: config
          mountPath: /app/config
        - name: secrets
          mountPath: /app/secrets
      volumes:
      - name: config
        configMap:
          name: price-feed-config
      - name: secrets
        secret:
          secretName: price-feed-secrets
```

### Service Configuration

Create a Kubernetes service in `k8s/service.yaml`:

```yaml
apiVersion: v1
kind: Service
metadata:
  name: price-feed
  namespace: neo-service-layer
spec:
  selector:
    app: price-feed
  ports:
  - port: 80
    targetPort: 3000
  type: ClusterIP
```

### ConfigMap

Create a ConfigMap for non-sensitive configuration in `k8s/configmap.yaml`:

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: price-feed-config
  namespace: neo-service-layer
data:
  price-feed.config.json: |
    {
      "service": {
        "name": "price-feed",
        "version": "1.2.0",
        "environment": "production"
      },
      "monitoring": {
        "prometheusPort": 9090,
        "logLevel": "info",
        "metricsPrefix": "price_feed"
      },
      "kalman": {
        "baseProcessNoise": 0.001,
        "baseMeasurementNoise": 0.1,
        "multiStateEnabled": true
      }
    }
```

### Secrets Management

Create a Kubernetes secret for sensitive data:

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: price-feed-secrets
  namespace: neo-service-layer
type: Opaque
data:
  BINANCE_API_KEY: <base64-encoded-key>
  HUOBI_API_KEY: <base64-encoded-key>
  NEO_RPC_URL: <base64-encoded-url>
```

## Deployment Process

### 1. Build and Push Docker Image

```bash
# Build the image
docker build -t neo-service-layer/price-feed:1.2.0 .

# Push to registry
docker push neo-service-layer/price-feed:1.2.0
```

### 2. Deploy to Kubernetes

```bash
# Create namespace
kubectl create namespace neo-service-layer

# Apply configurations
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secrets.yaml
kubectl apply -f k8s/price-feed.yaml
kubectl apply -f k8s/service.yaml
```

### 3. Verify Deployment

```bash
# Check deployment status
kubectl get deployments -n neo-service-layer

# Check pods
kubectl get pods -n neo-service-layer

# Check logs
kubectl logs -f deployment/price-feed -n neo-service-layer
```

## Monitoring Setup

### Prometheus Configuration

Create `prometheus/price-feed-rules.yaml`:

```yaml
groups:
- name: price-feed
  rules:
  - alert: HighErrorRate
    expr: rate(price_feed_errors_total[5m]) > 0.01
    for: 5m
    labels:
      severity: critical
    annotations:
      summary: High error rate in price feed
      
  - alert: LowConfidence
    expr: price_feed_confidence < 0.8
    for: 5m
    labels:
      severity: warning
    annotations:
      summary: Low confidence in price data
      
  - alert: HighLatency
    expr: rate(price_feed_request_duration_seconds_sum[5m]) > 1
    for: 5m
    labels:
      severity: warning
    annotations:
      summary: High latency in price updates
```

### Grafana Dashboard

Create a Grafana dashboard for monitoring:

```json
{
  "dashboard": {
    "title": "Price Feed Monitoring",
    "panels": [
      {
        "title": "Price Updates",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(price_feed_updates_total[5m])"
          }
        ]
      },
      {
        "title": "Confidence Levels",
        "type": "gauge",
        "targets": [
          {
            "expr": "price_feed_confidence"
          }
        ]
      },
      {
        "title": "Error Rates",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(price_feed_errors_total[5m])"
          }
        ]
      }
    ]
  }
}
```

## Scaling Configuration

### Horizontal Pod Autoscaling

Create `k8s/hpa.yaml`:

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: price-feed-hpa
  namespace: neo-service-layer
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: price-feed
  minReplicas: 3
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

## Backup and Recovery

### Backup Configuration

Create a CronJob for regular backups:

```yaml
apiVersion: batch/v1
kind: CronJob
metadata:
  name: price-feed-backup
  namespace: neo-service-layer
spec:
  schedule: "0 */6 * * *"  # Every 6 hours
  jobTemplate:
    spec:
      template:
        spec:
          containers:
          - name: backup
            image: neo-service-layer/backup:latest
            env:
            - name: BACKUP_PATH
              value: "/backups"
            volumeMounts:
            - name: backup-volume
              mountPath: /backups
          volumes:
          - name: backup-volume
            persistentVolumeClaim:
              claimName: backup-pvc
          restartPolicy: OnFailure
```

## Maintenance Procedures

### Rolling Updates

To perform a rolling update:

```bash
# Update image version
kubectl set image deployment/price-feed \
  price-feed=neo-service-layer/price-feed:1.2.1 \
  -n neo-service-layer

# Monitor rollout
kubectl rollout status deployment/price-feed -n neo-service-layer
```

### Rollback Procedure

If issues are detected:

```bash
# Rollback to previous version
kubectl rollout undo deployment/price-feed -n neo-service-layer

# Verify rollback
kubectl rollout status deployment/price-feed -n neo-service-layer
```

## Security Considerations

### Network Policies

Create `k8s/network-policy.yaml`:

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: price-feed-network-policy
  namespace: neo-service-layer
spec:
  podSelector:
    matchLabels:
      app: price-feed
  policyTypes:
  - Ingress
  - Egress
  ingress:
  - from:
    - namespaceSelector:
        matchLabels:
          name: monitoring
    ports:
    - protocol: TCP
      port: 9090
  egress:
  - to:
    - ipBlock:
        cidr: 0.0.0.0/0
        except:
        - 169.254.0.0/16
        - 172.16.0.0/12
    ports:
    - protocol: TCP
      port: 443
```

### Pod Security Policy

Create `k8s/pod-security-policy.yaml`:

```yaml
apiVersion: policy/v1beta1
kind: PodSecurityPolicy
metadata:
  name: price-feed-psp
spec:
  privileged: false
  seLinux:
    rule: RunAsAny
  runAsUser:
    rule: MustRunAsNonRoot
  fsGroup:
    rule: RunAsAny
  volumes:
  - configMap
  - secret
  - emptyDir
```

## Troubleshooting

### Common Issues

1. **Pod Startup Failures**
   ```bash
   # Check pod status
   kubectl describe pod <pod-name> -n neo-service-layer
   
   # Check container logs
   kubectl logs <pod-name> -n neo-service-layer
   ```

2. **High Error Rates**
   ```bash
   # Check error metrics
   curl http://<service-ip>:9090/metrics | grep price_feed_errors
   
   # Check recent error logs
   kubectl logs -l app=price-feed -n neo-service-layer --tail=100
   ```

3. **Performance Issues**
   ```bash
   # Check resource usage
   kubectl top pods -n neo-service-layer
   
   # Check network metrics
   kubectl exec <pod-name> -n neo-service-layer -- netstat -an
   ```

## References

1. [Kubernetes Documentation](https://kubernetes.io/docs/)
2. [Neo N3 Node Setup](https://docs.neo.org/docs/en-us/node/cli/setup.html)
3. [Prometheus Operator](https://prometheus-operator.dev/)
4. [Grafana Documentation](https://grafana.com/docs/) 