<template>
  <div class="notifications-panel">
    <div class="header">
      <h3>Уведомления</h3>
      <span class="badge" v-if="hasUnread">{{ unreadCount }}</span>
      <button class="btn-clear" @click="clearAll">Очистить</button>
    </div>

    <div class="messages">
      <div v-if="messages.length === 0" class="empty">
        Нет уведомлений
      </div>
      <div
        v-for="(msg, index) in messages"
        :key="index"
        class="message"
        :class="msg.type"
      >
        <span class="time">{{ formatTime(msg.timestamp) }}</span>
        <span class="text">{{ msg.text }}</span>
      </div>
    </div>
  </div>
</template>

<script setup>
import { storeToRefs } from 'pinia';
import { useNotificationsStore } from '@/stores/notifications';

const store = useNotificationsStore();
const { messages, unreadCount, hasUnread } = storeToRefs(store);
const { clearAll } = store;

const formatTime = (timestamp) => {
  const date = new Date(timestamp);
  return date.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });
};
</script>

<style scoped>
.notifications-panel {
  background: white;
  border-radius: 10px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
  max-width: 400px;
  max-height: 400px;
  display: flex;
  flex-direction: column;
}

.header {
  padding: 15px;
  border-bottom: 1px solid #eee;
  display: flex;
  align-items: center;
  gap: 10px;
}

.header h3 {
  margin: 0;
  font-size: 1rem;
}

.badge {
  background: #e74c3c;
  color: white;
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 0.7rem;
}

.btn-clear {
  margin-left: auto;
  background: none;
  border: none;
  color: #999;
  cursor: pointer;
  font-size: 0.8rem;
}

.btn-clear:hover {
  color: #333;
}

.messages {
  flex: 1;
  overflow-y: auto;
  padding: 10px;
}

.empty {
  color: #999;
  text-align: center;
  padding: 20px;
}

.message {
  padding: 8px 10px;
  margin: 4px 0;
  border-radius: 5px;
  font-size: 0.9rem;
  display: flex;
  gap: 8px;
  align-items: center;
}

.message .time {
  color: #999;
  font-size: 0.7rem;
  white-space: nowrap;
}

.message.system {
  background: #f5f5f5;
}

.message.news {
  background: #e3f2fd;
  border-left: 3px solid #2196f3;
}

.message.topic {
  background: #e8f5e9;
  border-left: 3px solid #4caf50;
}

.message.important {
  background: #fce4ec;
  border-left: 3px solid #f44336;
  font-weight: bold;
}
</style>