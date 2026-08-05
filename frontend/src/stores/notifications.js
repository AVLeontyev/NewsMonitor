import { defineStore } from 'pinia';

export const useNotificationsStore = defineStore('notifications', {
  state: () => ({
    messages: [],
    unreadCount: 0,
  }),

  actions: {
    addMessage(message) {
      this.messages.unshift(message);
      this.unreadCount++;
    },

    markAllAsRead() {
      this.unreadCount = 0;
    },

    clearAll() {
      this.messages = [];
      this.unreadCount = 0;
    },
  },

  getters: {
    recentMessages: (state) => state.messages.slice(0, 20),
    hasUnread: (state) => state.unreadCount > 0,
  },
});