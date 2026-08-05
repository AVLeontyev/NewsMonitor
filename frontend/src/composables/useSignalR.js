import { ref, onMounted, onUnmounted } from 'vue';
import * as signalR from '@microsoft/signalr';

export function useSignalR() {
  const connection = ref(null);
  const isConnected = ref(false);
  const messages = ref([]);
  const connectionId = ref('');

  const startConnection = async () => {
    try {
      connection.value = new signalR.HubConnectionBuilder()
        .withUrl('http://localhost:5269/newshub')
        .configureLogging(signalR.LogLevel.Information)
        .build();

      // Обработчики событий
      connection.value.on('Connected', (message) => {
        console.log('Connected:', message);
        addSystemMessage(message);
      });

      connection.value.on('ReceiveNews', (message) => {
        console.log('ReceiveNews:', message);
        addNewsMessage(message, 'news');
      });

      connection.value.on('ReceiveTopicNews', (message) => {
        console.log('ReceiveTopicNews:', message);
        addNewsMessage(message, 'topic');
      });

      connection.value.on('ReceiveImportantNews', (message) => {
        console.log('ReceiveImportantNews:', message);
        addNewsMessage(message, 'important');
      });

      connection.value.on('Subscribed', (message) => {
        console.log('Subscribed:', message);
        addSystemMessage(`${message}`);
      });

      connection.value.on('Unsubscribed', (message) => {
        console.log('Unsubscribed:', message);
        addSystemMessage(`${message}`);
      });

      await connection.value.start();
      isConnected.value = true;
      connectionId.value = connection.value.connectionId;
      addSystemMessage('Подключено к SignalR');

    } catch (error) {
      console.error('Connection error:', error);
      addSystemMessage(`Ошибка подключения: ${error.message}`);
    }
  };

  const stopConnection = async () => {
    if (connection.value) {
      await connection.value.stop();
      isConnected.value = false;
      connectionId.value = '';
      addSystemMessage('Отключено от SignalR');
    }
  };

  const subscribeToTopic = async (topic) => {
    if (!connection.value || !isConnected.value) {
      addSystemMessage('Сначала подключитесь к SignalR');
      return;
    }
    try {
      await connection.value.invoke('SubscribeToTopic', topic);
    } catch (error) {
      console.error('Subscribe error:', error);
      addSystemMessage(`Ошибка подписки: ${error.message}`);
    }
  };

  const unsubscribeFromTopic = async (topic) => {
    if (!connection.value || !isConnected.value) return;
    try {
      await connection.value.invoke('UnsubscribeFromTopic', topic);
    } catch (error) {
      console.error('Unsubscribe error:', error);
    }
  };

  const addNewsMessage = (message, type) => {
    messages.value.push({
      type: type,
      topic: message.topic,
      title: message.title,
      description: message.description || '',
      sourceUrl: message.sourceUrl || '',
      timestamp: message.timestamp || new Date().toISOString(),
      isImportant: type === 'important',
    });
  };

  const addSystemMessage = (text) => {
    messages.value.push({
      type: 'system',
      title: text,
      timestamp: new Date().toISOString(),
    });
  };

  onUnmounted(() => {
    if (connection.value) {
      connection.value.stop();
    }
  });

  return {
    connection,
    isConnected,
    messages,
    connectionId,
    startConnection,
    stopConnection,
    subscribeToTopic,
    unsubscribeFromTopic,
  };
}