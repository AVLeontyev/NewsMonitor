<template>
  <div id="app">
    <header class="header">
      <div class="logo">
        <h1>News Monitor</h1>
      </div>
      <div class="status">
        <span class="badge" :class="signalrStore.isConnected ? 'connected' : 'disconnected'">
          {{ signalrStore.isConnected ? '🟢 Онлайн' : '🔴 Офлайн' }}
        </span>
        <span v-if="signalrStore.connectionId" class="connection-id">
          ID: {{ signalrStore.connectionId }}
        </span>
      </div>
    </header>

    <div class="container">
      <nav class="nav">
        <router-link to="/" class="nav-link">Главная</router-link>
        <router-link to="/topics" class="nav-link">Темы</router-link>
        <router-link to="/news" class="nav-link">Новости</router-link>
      </nav>

      <main>
        <router-view />
      </main>
    </div>
  </div>
</template>

<script setup>
import { onMounted, onUnmounted } from 'vue';
import { useSignalRStore } from './stores/signalr';

const signalrStore = useSignalRStore();

onMounted(() => {
  signalrStore.connect();
});

onUnmounted(() => {
  signalrStore.disconnect();
});
</script>

<style>
/* Глобальные стили — без scoped */
* {
  margin: 0;
  padding: 0;
  box-sizing: border-box;
}

body {
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  background: #f5f6fa;
  color: #333;
}

.header {
  background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%);
  color: white;
  padding: 20px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.logo h1 {
  margin: 0;
  font-size: 1.5rem;
}

.badge {
  padding: 5px 15px;
  border-radius: 20px;
  font-size: 0.9rem;
  font-weight: bold;
}

.connected {
  background: #27ae60;
}

.disconnected {
  background: #e74c3c;
}

.connection-id {
  font-size: 0.8rem;
  opacity: 0.7;
  margin-left: 10px;
}

.container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 20px;
}

.nav {
  display: flex;
  gap: 10px;
  margin-bottom: 20px;
  border-bottom: 2px solid #eee;
  padding-bottom: 10px;
}

.nav-link {
  padding: 8px 20px;
  color: #333;
  text-decoration: none;
  border-radius: 5px;
  transition: all 0.3s;
}

.nav-link:hover {
  background: #f0f0f0;
}

.nav-link.router-link-active {
  background: #3498db;
  color: white;
}

/* Глобальные стили кнопок */
.btn {
  padding: 10px 20px;
  border: none;
  border-radius: 5px;
  cursor: pointer;
  font-size: 14px;
  transition: all 0.3s;
}

.btn:hover {
  opacity: 0.9;
  transform: translateY(-1px);
}

.btn:active {
  transform: translateY(0);
}

.btn-primary {
  background: #3498db;
  color: white;
}

.btn-success {
  background: #27ae60;
  color: white;
}

.btn-danger {
  background: #e74c3c;
  color: white;
}

/* Поля ввода */
input[type="text"] {
  padding: 10px 15px;
  border: 1px solid #ddd;
  border-radius: 5px;
  font-size: 14px;
  width: 300px;
}

input[type="text"]:focus {
  outline: none;
  border-color: #3498db;
  box-shadow: 0 0 0 2px rgba(52, 152, 219, 0.2);
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

.status-bar {
  padding: 15px;
  background: #f8f9fa;
  border-radius: 5px;
  margin: 10px 0;
  display: flex;
  gap: 15px;
  align-items: center;
}
</style>