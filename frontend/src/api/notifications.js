import api from './client';

export const notificationsApi = {
  send(data) {
    return api.post('/notifications', data);
  },
};