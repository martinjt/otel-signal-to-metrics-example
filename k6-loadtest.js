import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');

// Test configuration
export const options = {
  stages: [
    { duration: '30s', target: 100 },  // Ramp up to 10 users over 30 seconds
    { duration: '1m', target: 200 },   // Ramp up to 20 users over 1 minute
    { duration: '2m', target: 100 },   // Stay at 20 users for 2 minutes
    { duration: '30s', target: 150 },   // Ramp down to 0 users
  ]
};

// Base URL for the API
const BASE_URL = 'http://localhost:8080';

export default function () {
  // Call the basket endpoint
  const basketResponse = http.get(`${BASE_URL}/basket`);

  // Check the response
  const basketSuccess = check(basketResponse, {
    'basket status is 200 or 500': (r) => r.status === 200 || r.status === 500,
    'basket status is 200': (r) => r.status === 200,
    'basket has value': (r) => {
      if (r.status === 200) {
        const body = JSON.parse(r.body);
        return body.value !== undefined && body.value > 0;
      }
      return true;
    },
  });

  // Record errors
  errorRate.add(!basketSuccess);
}