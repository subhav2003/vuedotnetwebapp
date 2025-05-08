<template>
  <div class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
    <div class="relative bg-[#1a1a1a] text-[#C8B280] rounded-lg shadow-xl w-full max-w-md max-h-[90vh] overflow-y-auto p-6">
      <button @click="$emit('close')" class="absolute top-4 right-4 text-[#aaa] hover:text-red-600 text-2xl">&times;</button>

      <Toast :toasts="toasts" @close="removeToast" />

      <h2 class="text-xl font-semibold mb-4">Add New Genre</h2>
      <form @submit.prevent="addGenre" class="space-y-4">
        <div>
          <label class="block mb-1">Genre Name</label>
          <input v-model="genre.name" type="text" class="input-field" required />
        </div>

        <button
            type="submit"
            :disabled="loading"
            class="w-full bg-green-700 text-white py-2 rounded hover:bg-green-600 transition flex items-center justify-center disabled:opacity-50"
        >
          <span v-if="loading" class="loader w-4 h-4 mr-2"></span>
          <span>{{ loading ? 'Adding...' : 'Add Genre' }}</span>
        </button>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue';
import axios from 'axios';
import Toast from '@/components/Toast.vue';

const apiBaseUrl = process.env.VUE_APP_API_URL;

const genre = reactive({ name: '' });
const loading = ref(false);
const toasts = reactive([]);

function removeToast(id) {
  const i = toasts.findIndex(t => t.id === id);
  if (i !== -1) toasts.splice(i, 1);
}
function showToast(message, type = 'success') {
  const id = Date.now() + Math.random();
  toasts.push({ id, message, type });
  setTimeout(() => removeToast(id), 5000);
}

async function addGenre() {
  loading.value = true;
  try {
    const res = await axios.post(`${apiBaseUrl}/api/Book/genres`, genre, {
      headers: { Authorization: `Bearer ${localStorage.getItem('authToken')}` }
    });

    if (res?.data?.success) {
      showToast(res.data.message || 'Genre added successfully!', 'success');
      genre.name = '';
    } else {
      showToast(res.data.message || 'Failed to add genre.', 'error');
    }
  } catch (err) {
    const msgs = err.response?.data?.errors;
    if (msgs) {
      Object.values(msgs).flat().forEach(m => showToast(m, 'error'));
    } else {
      showToast(err.response?.data?.message || 'An unexpected error occurred.', 'error');
    }
  } finally {
    loading.value = false;
  }
}
</script>

<style scoped>
.input-field {
  width: 100%;
  padding: .5rem;
  background-color: #1f1f1f;
  color: #C8B280;
  border: 1px solid #3a3a3a;
  border-radius: .375rem;
  margin-top: .25rem;
  outline: none;
  transition: border-color .2s, background-color .2s;
}
.input-field:focus {
  border-color: #10b981;
  background-color: #2a2a2a;
}
.loader {
  border-top: 3px solid #f3f3f3;
  border-right: 3px solid #10b981;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}
@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>
