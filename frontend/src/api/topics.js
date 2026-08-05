import api from './client';

export const topicsApi = {
  // Получить все темы
  getAll() {
    return api.get('/topics');
  },

  // Создать тему
  create(data) {
    return api.post('/topics', data);
  },

  // Удалить тему
  delete(id) {
    return api.delete(`/topics/${id}`);
  },

  // Обновить тему
  update(id, data) {
    return api.put(`/topics/${id}`, data);
  },
};