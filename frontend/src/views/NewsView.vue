<template>
  <div class="news-page">
    <h2>Новости</h2>

    <!-- Фильтры -->
    <div class="filters">
      <div class="filter-group">
        <label>Тема:</label>
        <select v-model="selectedTopic" @change="applyFilters">
          <option value="">Все темы</option>
          <option v-for="topic in topicsStore.topics" :key="topic.id" :value="topic.name">
            {{ topic.name }}
          </option>
        </select>
      </div>

      <div class="filter-group">
        <label>
          <input type="checkbox" v-model="showImportant" @change="applyFilters" />
          Только важные
        </label>
      </div>

      <div class="filter-group">
        <button @click="clearFilters" class="btn btn-secondary">Сбросить фильтры</button>
      </div>

      <div class="filter-group stats">
        <span class="stat">Всего: {{ newsStore.news.length }}</span>
        <span class="stat" v-if="unreadCount > 0">Непрочитанных: {{ unreadCount }}</span>
        <span class="stat important-count" v-if="importantCount > 0">Важных: {{ importantCount }}</span>
      </div>
    </div>

    <!-- Список новостей -->
    <div class="news-list">
      <div v-if="newsStore.loading" class="loading">
        <div class="spinner"></div>
        <span>Загрузка новостей...</span>
      </div>

      <div v-else-if="newsStore.news.length === 0" class="empty">
        Новостей пока нет
        <p>Подпишитесь на темы, чтобы получать новости</p>
      </div>

      <div
        v-for="news in newsStore.news"
        :key="news.id"
        class="news-item"
        :class="{ important: news.isImportant, read: news.isRead }"
        @click="markAsRead(news.id)"
      >
        <div class="news-header">
          <div class="news-title">
            <span class="badge" v-if="news.isImportant">⭐ Важно</span>
            <span class="badge topic-badge">{{ news.topic }}</span>
            <span class="badge source-badge" v-if="news.sourceName">{{ news.sourceName }}</span>
          </div>
          <span class="news-date">{{ formatDate(news.publishedAt || news.createdAt) }}</span>
        </div>

        <h3 class="news-title-text">{{ news.title }}</h3>

        <p class="news-description" v-if="news.description">
          {{ truncateText(news.description, 200) }}
        </p>

        <div class="news-footer">
          <a v-if="news.sourceUrl" :href="news.sourceUrl" target="_blank" class="source-link">
            🔗 Источник
          </a>
          <span class="news-id">ID: {{ news.id.slice(0, 8) }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue';
import { useNewsStore } from '@/stores/news';
import { useTopicsStore } from '@/stores/topics';

const newsStore = useNewsStore();
const topicsStore = useTopicsStore();

const selectedTopic = ref('');
const showImportant = ref(false);

const unreadCount = computed(() => newsStore.unreadCount);
const importantCount = computed(() => newsStore.importantCount);

const applyFilters = () => {
  const params = {};
  if (selectedTopic.value) {
    params.topic = selectedTopic.value;
  }
  if (showImportant.value) {
    params.important = true;
  }
  newsStore.fetchNews(params);
};

const clearFilters = () => {
  selectedTopic.value = '';
  showImportant.value = false;
  newsStore.fetchNews();
};

const markAsRead = (id) => {
  newsStore.markAsRead(id);
};

const formatDate = (dateStr) => {
  if (!dateStr) return '';
  const date = new Date(dateStr);
  const now = new Date();
  const diff = now - date;

  if (diff < 60000) return 'Только что';
  if (diff < 3600000) return `${Math.floor(diff / 60000)} мин. назад`;
  if (diff < 86400000) return `${Math.floor(diff / 3600000)} ч. назад`;
  if (diff < 604800000) return `${Math.floor(diff / 86400000)} дн. назад`;

  return date.toLocaleDateString('ru-RU', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
};

const truncateText = (text, length) => {
  if (text.length <= length) return text;
  return text.slice(0, length) + '...';
};

onMounted(async () => {
  await topicsStore.fetchTopics();
  await newsStore.fetchNews();
});

// Обновляем новости при появлении новых через SignalR
import { useSignalRStore } from '@/stores/signalr';
const signalrStore = useSignalRStore();

watch(
  () => signalrStore.messages.length,
  () => {
    // Если пришло новое уведомление о новости, обновляем список
    const lastMessage = signalrStore.messages[signalrStore.messages.length - 1];
    if (lastMessage && (lastMessage.type === 'news' || lastMessage.type === 'important')) {
      newsStore.fetchNews();
    }
  }
);
</script>

<style scoped>
.news-page {
  padding: 20px;
}

.filters {
  display: flex;
  flex-wrap: wrap;
  gap: 15px;
  padding: 15px 20px;
  background: #f8f9fa;
  border-radius: 8px;
  margin: 15px 0 25px 0;
  align-items: center;
}

.filter-group {
  display: flex;
  align-items: center;
  gap: 8px;
}

.filter-group label {
  font-size: 14px;
  color: #555;
}

.filter-group select {
  padding: 6px 12px;
  border: 1px solid #ddd;
  border-radius: 5px;
  background: white;
  font-size: 14px;
}

.filter-group input[type="checkbox"] {
  width: 18px;
  height: 18px;
  cursor: pointer;
}

.btn-secondary {
  padding: 6px 15px;
  background: #6c757d;
  color: white;
  border: none;
  border-radius: 5px;
  cursor: pointer;
  font-size: 14px;
  transition: all 0.2s;
}

.btn-secondary:hover {
  background: #5a6268;
}

.stats {
  margin-left: auto;
  gap: 15px;
}

.stat {
  font-size: 14px;
  color: #555;
}

.important-count {
  color: #e74c3c;
  font-weight: bold;
}

.news-list {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.news-item {
  background: white;
  border-radius: 8px;
  padding: 20px;
  border: 1px solid #e0e0e0;
  cursor: pointer;
  transition: all 0.2s;
}

.news-item:hover {
  box-shadow: 0 2px 8px rgba(0,0,0,0.08);
  border-color: #ccc;
}

.news-item.important {
  border-left: 4px solid #e74c3c;
}

.news-item.read {
  opacity: 0.7;
}

.news-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 10px;
  flex-wrap: wrap;
  gap: 8px;
}

.news-title {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
  align-items: center;
}

.badge {
  padding: 2px 10px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: bold;
}

.badge.topic-badge {
  background: #e3f2fd;
  color: #1565c0;
}

.badge.source-badge {
  background: #f3e5f5;
  color: #7b1fa2;
}

.news-title-text {
  margin: 0 0 10px 0;
  font-size: 1.1rem;
  color: #2c3e50;
}

.news-description {
  color: #555;
  font-size: 0.95rem;
  line-height: 1.5;
  margin: 8px 0;
}

.news-date {
  color: #999;
  font-size: 12px;
  white-space: nowrap;
}

.news-footer {
  margin-top: 12px;
  padding-top: 12px;
  border-top: 1px solid #eee;
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 12px;
  color: #999;
}

.source-link {
  color: #3498db;
  text-decoration: none;
}

.source-link:hover {
  text-decoration: underline;
}

.loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 40px;
  color: #999;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #f3f3f3;
  border-top: 4px solid #3498db;
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 15px;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

.empty {
  text-align: center;
  padding: 60px 20px;
  color: #999;
  font-size: 1.2rem;
}

.empty p {
  margin-top: 10px;
  font-size: 0.9rem;
  color: #bbb;
}
</style>