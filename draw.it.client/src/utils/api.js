import axios from 'axios'

const BASE_URL = 'http://localhost:5094';

const instance = axios.create({
    baseURL: BASE_URL
});

const api = {
    get: instance.get,
    post: instance.post,
    put: instance.put,
    delete: instance.delete,
    patch: instance.patch
};

export default api;


