const express = require('express');
const redis = require('redis');
const cors = require('cors');

const app = express();
app.use(cors());
app.use(express.json());

const redisClient = redis.createClient({
    socket: {
        host: process.env.REDIS_HOST || 'redis',
        port: process.env.REDIS_PORT || 6379
    }
});

redisClient.on('error', (err) => console.error('Redis Client Error', err));

(async () => {
    await redisClient.connect();
    console.log('Connected to Redis');
})();

// Validate invite code and return server info
app.get('/api/lobby/:code', async (req, res) => {
    try {
        const code = req.params.code;
        const serverInfo = await redisClient.get(`invite:${code}`);
        
        if (!serverInfo) {
            return res.status(404).json({ error: 'Invalid or expired invite code' });
        }

        const [ip, port] = serverInfo.split(':');
        res.json({ ip, port });
    } catch (error) {
        console.error('Error:', error);
        res.status(500).json({ error: 'Internal server error' });
    }
});

// Create new lobby invite
app.post('/api/lobby', async (req, res) => {
    try {
        const { code, ip, port, ttl } = req.body;
        
        if (!code || !ip || !port) {
            return res.status(400).json({ error: 'Missing required fields' });
        }

        const serverInfo = `${ip}:${port}`;
        const ttlSeconds = ttl || 3600; // Default 1 hour
        
        await redisClient.setEx(`invite:${code}`, ttlSeconds, serverInfo);
        
        res.json({ success: true, code, ip, port });
    } catch (error) {
        console.error('Error:', error);
        res.status(500).json({ error: 'Internal server error' });
    }
});

// Get next instance number
app.post('/api/instance/next', async (req, res) => {
    try {
        const instance = await redisClient.incr('lobby_counter');
        res.json({ instance });
    } catch (error) {
        console.error('Error:', error);
        res.status(500).json({ error: 'Internal server error' });
    }
});

// List all active lobbies (for debugging)
app.get('/api/lobbies', async (req, res) => {
    try {
        const keys = await redisClient.keys('invite:*');
        const lobbies = [];
        
        for (const key of keys) {
            const serverInfo = await redisClient.get(key);
            const code = key.replace('invite:', '');
            const [ip, port] = serverInfo.split(':');
            const ttl = await redisClient.ttl(key);
            
            lobbies.push({ code, ip, port, ttl });
        }
        
        res.json({ lobbies });
    } catch (error) {
        console.error('Error:', error);
        res.status(500).json({ error: 'Internal server error' });
    }
});

// Health check endpoint
app.get('/health', (req, res) => {
    res.json({ status: 'ok', timestamp: new Date().toISOString() });
});

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
    console.log(`API server running on port ${PORT}`);
});
