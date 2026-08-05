<template>
  <div class="home">
    <h2>Главная</h2>

    <div class="status-bar">
      <span class="badge" :class="signalrStore.isConnected ? 'connected' : 'disconnected'">
        {{ signalrStore.isConnected ? '🟢 Онлайн' : '🔴 Офлайн' }}
      </span>
      <span>Connection ID: {{ signalrStore.connectionId || 'Нет' }}</span>
    </div>

    <div class="controls">
      <div class="topic-control">
        <input
          v-model="topicName"
          placeholder="Название темы (например: Star Wars)"
          @keyup.enter="subscribe"
        />
        <button @click="subscribe" class="btn btn-primary">Подписаться</button>
        <button @click="unsubscribe" class="btn btn-danger">Отписаться</button>
      </div>

      <div class="test-control">
        <button @click="sendTestNotification" class="btn btn-success">
          Отправить тестовое уведомление
        </button>
      </div>
    </div>

    <!-- Лента сообщений SignalR -->
    <div class="messages-panel">
      <div class="messages-header">
        <h3>Сообщения SignalR</h3>
        <button @click="signalrStore.clearMessages" class="btn-clear">Очистить</button>
      </div>
      <div class="messages-list" ref="messagesContainer">
        <div v-if="!signalrStore.hasMessages" class="no-messages">
          Нет сообщений. Подпишитесь на тему или отправьте тестовое уведомление.
        </div>
        <div
          v-for="(msg, index) in signalrStore.messages"
          :key="index"
          class="message-item"
          :class="'msg-' + msg.type"
        >
          <span class="msg-time">{{ formatTime(msg.timestamp) }}</span>
          <span class="msg-type">{{ getTypeLabel(msg.type) }}</span>
          <span class="msg-text">{{ msg.text }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch, nextTick } from 'vue';
import { useSignalRStore } from '@/stores/signalr';
import { notificationsApi } from '@/api/notifications';

const signalrStore = useSignalRStore();
const topicName = ref('');
const messagesContainer = ref(null);

// Автопрокрутка вниз при новых сообщениях
watch(() => signalrStore.messages.length, async () => {
  await nextTick();
  if (messagesContainer.value) {
    messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight;
  }
});

const formatTime = (timestamp) => {
  const date = new Date(timestamp);
  return date.toLocaleTimeString('ru-RU');
};

const getTypeLabel = (type) => {
  const labels = {
    system: '📡',
    news: '📰',
    topic: '📌',
    important: '⚠️',
  };
  return labels[type] || '📝';
};

const subscribe = async () => {
  if (!topicName.value.trim()) {
    alert('Введите название темы');
    return;
  }
  await signalrStore.subscribeToTopic(topicName.value.trim());
};

const unsubscribe = async () => {
  if (!topicName.value.trim()) {
    alert('Введите название темы');
    return;
  }
  await signalrStore.unsubscribeFromTopic(topicName.value.trim());
};

const sendTestNotification = async () => {
  try {
    await notificationsApi.send({
      topic: 'Star Wars',
      title: 'Тестовое уведомление из Vue!',
      description: 'Это тестовое уведомление из Vue приложения',
      sourceUrl: 'https://example.com',
    });
  } catch (error) {
    console.error('Error sending notification:', error);
  }
};
</script>

<style scoped>
.status-bar {
  padding: 15px;
  background: #f8f9fa;
  border-radius: 8px;
  margin: 10px 0;
  display: flex;
  gap: 15px;
  align-items: center;
}

.badge {
  padding: 5px 15px;
  border-radius: 20px;
  font-size: 0.9rem;
  font-weight: bold;
  color: white;
}

.connected {
  background: #27ae60;
}

.disconnected {
  background: #e74c3c;
}

.controls {
  display: flex;
  flex-direction: column;
  gap: 15px;
  margin: 20px 0;
}

.topic-control, .test-control {
  display: flex;
  gap: 10px;
  align-items: center;
}

input {
  padding: 10px 15px;
  border: 1px solid #ddd;
  border-radius: 5px;
  font-size: 14px;
  width: 300px;
}

input:focus {
  outline: none;
  border-color: #3498db;
  box-shadow: 0 0 0 2px rgba(52, 152, 219, 0.2);
}

.btn {
  padding: 10px 20px;
  border: none;
  border-radius: 5px;
  cursor: pointer;
  font-size: 14px;
  transition: all 0.2s;
  color: white;
}

.btn:hover {
  opacity: 0.9;
  transform: translateY(-1px);
}

.btn-primary { background: #3498db; }
.btn-success { background: #27ae60; }
.btn-danger { background: #e74c3c; }

.messages-panel {
  margin-top: 25px;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  overflow: hidden;
}

.messages-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 15px;
  background: #f8f9fa;
  border-bottom: 1px solid #e0e0e0;
}

.messages-header h3 {
  margin: 0;
  font-size: 1.1rem;
}

.btn-clear {
  padding: 5px 15px;
  border: 1px solid #ccc;
  border-radius: 5px;
  background: white;
  cursor: pointer;
  font-size: 0.85rem;
}

.btn-clear:hover {
  background: #f0f0f0;
}

.messages-list {
  max-height: 400px;
  overflow-y: auto;
  padding: 10px;
  background: white;
}

.no-messages {
  text-align: center;
  color: #999;
  padding: 30px;
  font-style: italic;
}

.message-item {
  display: flex;
  gap: 10px;
  padding: 8px 10px;
  border-radius: 5px;
  margin-bottom: 4px;
  font-size: 0.9rem;
  align-items: baseline;
}

.message-item:hover {
  background: #f8f9fa;
}

.msg-time {
  color: #999;
  font-size: 0.8rem;
  min-width: 70px;
  font-family: monospace;
}

.msg-type {
  font-size: 0.9rem;
}

.msg-text {
  flex: 1;
}

.msg-system { background: #f0f4ff; }
.msg-news { background: #f0fff4; }
.msg-topic { background: #fff8f0; }
.msg-important { background: #fff0f0; }
</style>