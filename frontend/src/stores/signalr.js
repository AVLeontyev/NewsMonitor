import { defineStore } from 'pinia';
import * as signalR from '@microsoft/signalr';

export const useSignalRStore = defineStore('signalr', {
  state: () => ({
    connection: null,
    isConnected: false,
    connectionId: '',
    messages: [],
  }),

  actions: {
    async connect() {
      try {
        this.connection = new signalR.HubConnectionBuilder()
          .withUrl('http://localhost:5269/newshub')
          .configureLogging(signalR.LogLevel.Information)
          .withAutomaticReconnect()
          .build();

        this.connection.on('Connected', (message) => {
          this.addMessage(message, 'system');
        });

        this.connection.on('ReceiveNews', (message) => {
          this.addMessage(`${message.title} (${message.topic})`, 'news');
        });

        this.connection.on('ReceiveTopicNews', (message) => {
          this.addMessage(`${message.title} (${message.topic})`, 'topic');
        });

        this.connection.on('ReceiveImportantNews', (message) => {
          this.addMessage(`ВАЖНО! ${message.title} (${message.topic})`, 'important');
        });

        this.connection.on('Subscribed', (message) => {
          this.addMessage(`${message}`, 'system');
        });

        this.connection.on('Unsubscribed', (message) => {
          this.addMessage(`${message}`, 'system');
        });

        this.connection.onreconnected((connectionId) => {
          this.isConnected = true;
          this.connectionId = connectionId;
          this.addMessage(`Переподключено! ID: ${connectionId}`, 'system');
        });

        await this.connection.start();
        this.isConnected = true;
        this.connectionId = this.connection.connectionId;
        this.addMessage('Подключено к SignalR', 'system');

        // Делаем методы доступными глобально для быстрого доступа
        window.subscribeToTopic = this.subscribeToTopic;
        window.unsubscribeFromTopic = this.unsubscribeFromTopic;

      } catch (error) {
        console.error('Connection error:', error);
        this.addMessage(`Ошибка: ${error.message}`, 'system');
        this.isConnected = false;
      }
    },

    async disconnect() {
      if (this.connection) {
        await this.connection.stop();
        this.isConnected = false;
        this.connectionId = '';
        this.addMessage('🔌 Отключено от SignalR', 'system');
      }
    },

    async subscribeToTopic(topic) {
      if (!this.connection || !this.isConnected) {
        this.addMessage('Сначала подключитесь к SignalR', 'system');
        return false;
      }
      try {
        await this.connection.invoke('SubscribeToTopic', topic);
        this.addMessage(`Подписка на тему: ${topic}`, 'system');
        return true;
      } catch (error) {
        console.error('Subscribe error:', error);
        this.addMessage(`Ошибка подписки: ${error.message}`, 'system');
        return false;
      }
    },

    async unsubscribeFromTopic(topic) {
      if (!this.connection || !this.isConnected) return false;
      try {
        await this.connection.invoke('UnsubscribeFromTopic', topic);
        this.addMessage(`Отписка от темы: ${topic}`, 'system');
        return true;
      } catch (error) {
        console.error('Unsubscribe error:', error);
        return false;
      }
    },

    addMessage(text, type = 'system') {
      this.messages.push({
        type,
        text,
        timestamp: new Date().toISOString(),
      });
    },

    clearMessages() {
      this.messages = [];
    },
  },

  getters: {
    hasMessages: (state) => state.messages.length > 0,
    recentMessages: (state) => state.messages.slice(0, 20),
  },
});