import axios from 'axios'

const BASE_URL = 'http://localhost:5094';

const api = axios.create({
    baseURL: BASE_URL + "/api/v1",
    withCredentials: true // Important for cookies
});

export default api;