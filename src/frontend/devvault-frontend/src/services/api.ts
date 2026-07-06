import axios from 'axios';

const api = axios.create({
// Make sure this port matches what your C# terminal says!
  baseURL: 'http://localhost:5116/api/v1', // Replace with your backend API URL
  headers: {
    'Content-Type': 'application/json',
  },
});

export default api;