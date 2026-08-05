import { defineStore } from 'pinia';
import { topicsApi } from '@/api/topics';

export const useTopicsStore = defineStore('topics', {
  state: () => ({
    topics: [],
    loading: false,
    error: null,
  }),

  actions: {
    async fetchTopics() {
      this.loading = true;
      try {
        const response = await topicsApi.getAll();
        this.topics = response.data;
      } catch (error) {
        this.error = error.message;
        console.error('Error fetching topics:', error);
      } finally {
        this.loading = false;
      }
    },

    async createTopic(data) {
      try {
        const response = await topicsApi.create(data);
        this.topics.push(response.data);
        return response.data;
      } catch (error) {
        this.error = error.message;
        console.error('Error creating topic:', error);
        throw error;
      }
    },

    async updateTopic(id, data) {
      try {
        const response = await topicsApi.update(id, data);
        const index = this.topics.findIndex(t => t.id === id);
        if (index !== -1) {
          this.topics[index] = response.data;
        }
        return response.data;
      } catch (error) {
        this.error = error.message;
        console.error('Error updating topic:', error);
        throw error;
      }
    },

    async deleteTopic(id) {
      try {
        await topicsApi.delete(id);
        this.topics = this.topics.filter(t => t.id !== id);
      } catch (error) {
        this.error = error.message;
        console.error('Error deleting topic:', error);
        throw error;
      }
    },
  },
});