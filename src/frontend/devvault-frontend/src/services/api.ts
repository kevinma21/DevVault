import axios from 'axios';

const api = axios.create({
// Make sure this port matches what your C# terminal says!
  baseURL: 'http://localhost:5116/api/v1', // Replace with your backend API URL
  headers: {
    'Content-Type': 'application/json',
  },
});

api.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('accessToken');

        // If the token exists, attach it to the Authorization header
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => {
        return Promise.reject(error);
    }
);

export default api;