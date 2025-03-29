import express from 'express';
import cors from 'cors';
import { withAuth, withOptionalAuth } from './middleware/auth';
import { Logger } from '@/utils/logger';

const app = express();
const logger = Logger.getInstance().child({ service: 'api-server' });

// Middleware
app.use(express.json());
app.use(cors());
app.use(express.urlencoded({ extended: true }));

// Logging middleware
app.use((req, res, next) => {
  logger.info(`${req.method} ${req.url}`, {
    method: req.method,
    url: req.url,
    ip: req.ip,
    userAgent: req.headers['user-agent']
  });
  next();
});

// Error handling middleware
app.use((err: Error, req: express.Request, res: express.Response, next: express.NextFunction) => {
  logger.error('API Error', { error: err });
  res.status(500).json({ error: 'Internal server error' });
});

export { app, withAuth, withOptionalAuth };
