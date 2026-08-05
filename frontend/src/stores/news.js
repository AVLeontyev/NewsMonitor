import { defineStore } from 'pinia';
import { newsApi } from '@/api/news';

export const useNewsStore = defineStore('news', {
  state: () => ({
    news: [],
    loading: false,
    error: null,
    filters: {
      topic: '',
      important: false,
    },
  }),

  actions: {
    async fetchNews(params = {}) {
      this.loading = true;
      this.error = null;
      try {
        const response = await newsApi.getAll({
          topic: params.topic || this.filters.topic || undefined,
          important: params.important ?? (this.filters.important || undefined),
        });
        this.news = response.data;
      } catch (error) {
        this.error = error.message;
        console.error('Error fetching news:', error);
      } finally {
        this.loading = false;
      }
    },

    async fetchByTopic(topic) {
      this.loading = true;
      try {
        const response = await newsApi.getByTopic(topic);
        this.news = response.data;
      } catch (error) {
        this.error = error.message;
        console.error('Error fetching news by topic:', error);
      } finally {
        this.loading = false;
      }
    },

    async fetchImportant() {
      this.loading = true;
      try {
        const response = await newsApi.getImportant();
        this.news = response.data;
      } catch (error) {
        this.error = error.message;
        console.error('Error fetching important news:', error);
      } finally {
        this.loading = false;
      }
    },

    setFilterTopic(topic) {
      this.filters.topic = topic;
      this.fetchNews();
    },

    toggleImportantFilter() {
      this.filters.important = !this.filters.important;
      this.fetchNews();
    },

    clearFilters() {
      this.filters = { topic: '', important: false };
      this.fetchNews();
    },

    markAsRead(id) {
      const newsItem = this.news.find(n => n.id === id);
      if (newsItem) {
        newsItem.isRead = true;
      }
      newsApi.markAsRead(id).catch(error => {
        console.error('Error marking as read:', error);
        // Откат при ошибке
        if (newsItem) {
          newsItem.isRead = false;
        }
      });
    },
  },

  getters: {
    unreadCount: (state) => state.news.filter(n => !n.isRead).length,
    importantCount: (state) => state.news.filter(n => n.isImportant).length,
    filteredNews: (state) => {
      let result = state.news;
      if (state.filters.topic) {
        result = result.filter(n => n.topic.includes(state.filters.topic));
      }
      if (state.filters.important) {
        result = result.filter(n => n.isImportant);
      }
      return result;
    },
  },
});