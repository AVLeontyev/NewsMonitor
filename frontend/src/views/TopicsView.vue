<template>
  <div class="topics">
    <h2>Управление темами</h2>

    <!-- Форма создания темы -->
    <div class="create-form">
      <input
        v-model="newTopic.name"
        placeholder="Название темы"
        @keyup.enter="createTopic"
      />
      <input
        v-model="newTopic.keywords"
        placeholder="Ключевые слова (через запятую)"
        @keyup.enter="createTopic"
      />
      <button @click="createTopic" class="btn btn-primary">➕ Создать тему</button>
    </div>

    <!-- Список тем -->
    <div class="topics-list">
      <div
        v-for="topic in topicsStore.topics"
        :key="topic.id"
        class="topic-item"
      >
        <div class="topic-info">
          <span class="topic-name">{{ topic.name }}</span>
          <span class="topic-keywords">{{ topic.keywords || 'Нет ключевых слов' }}</span>
          <span class="topic-status" :class="topic.isActive ? 'active' : 'inactive'">
            {{ topic.isActive ? '✅ Активна' : '❌ Неактивна' }}
          </span>
        </div>
        <div class="topic-actions">
          <button @click="toggleTopic(topic)" class="btn btn-sm" :class="topic.isActive ? 'btn-warning' : 'btn-success'">
            {{ topic.isActive ? 'Деактивировать' : 'Активировать' }}
          </button>
          <button @click="deleteTopic(topic.id)" class="btn btn-sm btn-danger">🗑️</button>
        </div>
      </div>

      <div v-if="topicsStore.loading" class="loading">
        Загрузка...
      </div>

      <div v-if="!topicsStore.loading && topicsStore.topics.length === 0" class="empty">
        Нет тем. Создайте первую тему!
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useTopicsStore } from '@/stores/topics';
import { useSignalRStore } from '@/stores/signalr';

const topicsStore = useTopicsStore();
const signalrStore = useSignalRStore();

const newTopic = ref({
  name: '',
  keywords: '',
});

const createTopic = async () => {
  if (!newTopic.value.name.trim()) {
    alert('Введите название темы');
    return;
  }

  try {
    await topicsStore.createTopic({
      name: newTopic.value.name.trim(),
      keywords: newTopic.value.keywords.trim(),
    });
    newTopic.value = { name: '', keywords: '' };
    await topicsStore.fetchTopics();
  } catch (error) {
    console.error('Error creating topic:', error);
    alert('Ошибка при создании темы');
  }
};

const deleteTopic = async (id) => {
  if (!confirm('Удалить тему?')) return;
  try {
    await topicsStore.deleteTopic(id);
    await topicsStore.fetchTopics();
  } catch (error) {
    console.error('Error deleting topic:', error);
  }
};

const toggleTopic = async (topic) => {
  try {
    await topicsStore.updateTopic(topic.id, {
      ...topic,
      isActive: !topic.isActive,
    });
    await topicsStore.fetchTopics();
  } catch (error) {
    console.error('Error toggling topic:', error);
  }
};

onMounted(() => {
  topicsStore.fetchTopics();
});
</script>

<style scoped>
.topics {
  padding: 20px;
}

.create-form {
  display: flex;
  gap: 10px;
  margin: 20px 0;
  flex-wrap: wrap;
}

.create-form input {
  flex: 1;
  min-width: 200px;
  padding: 10px 15px;
  border: 1px solid #ddd;
  border-radius: 5px;
  font-size: 14px;
}

.btn {
  padding: 10px 20px;
  border: none;
  border-radius: 5px;
  cursor: pointer;
  font-size: 14px;
  color: white;
  transition: all 0.2s;
}

.btn:hover {
  opacity: 0.9;
  transform: translateY(-1px);
}

.btn-primary { background: #3498db; }
.btn-success { background: #27ae60; }
.btn-danger { background: #e74c3c; }
.btn-warning { background: #f39c12; }
.btn-sm { padding: 5px 12px; font-size: 12px; }

.topics-list {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.topic-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 15px;
  background: white;
  border-radius: 8px;
  border: 1px solid #e0e0e0;
  transition: all 0.2s;
}

.topic-item:hover {
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.topic-info {
  display: flex;
  gap: 15px;
  align-items: center;
  flex-wrap: wrap;
}

.topic-name {
  font-weight: bold;
  font-size: 1.1rem;
}

.topic-keywords {
  color: #666;
  font-size: 0.9rem;
}

.topic-status {
  font-size: 0.8rem;
  padding: 2px 10px;
  border-radius: 12px;
}

.topic-status.active {
  background: #d4edda;
  color: #155724;
}

.topic-status.inactive {
  background: #f8d7da;
  color: #721c24;
}

.topic-actions {
  display: flex;
  gap: 8px;
}

.loading, .empty {
  text-align: center;
  padding: 40px;
  color: #999;
}
</style>