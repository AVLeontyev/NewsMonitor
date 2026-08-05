import api from './client';

export const newsApi = {
  // Получить все новости
  getAll(params) {
    return api.get('/news', { params });
  },

  // Получить новости по теме
  getByTopic(topicId) {
    return api.get(`/news/topic/${topicId}`);
  },

  // Получить важные новости
  getImportant() {
    return api.get('/news/important');
  },

  // Отметить новость как прочитанную
  markAsRead(id) {
    return api.patch(`/news/${id}/read`);
  },
};