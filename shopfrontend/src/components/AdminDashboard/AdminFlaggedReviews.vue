<template>
  <div class="bg-white shadow-lg rounded-lg p-6 w-full">
    <!-- Toast stack -->
    <Toast :toasts="toasts" @close="removeToast" />

    <!-- Header -->
    <div class="flex items-center justify-between mb-6">
      <h2 class="text-3xl font-semibold text-gray-800">Flagged Reviews</h2>
      <button @click="fetchFlaggedReviews" class="flex items-center text-gray-600 hover:text-gray-800 transition">
        <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.253 6A9 9 0 115.334 5.334M20 20v-5h-.581" />
        </svg>
        Reload
      </button>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="flex justify-center items-center py-6">
      <svg class="animate-spin h-8 w-8 text-gray-700" fill="none" viewBox="0 0 24 24">
        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
      </svg>
      <span class="ml-2 text-gray-700 font-medium">Loading flagged reviews...</span>
    </div>

    <!-- Error -->
    <p v-if="error" class="text-red-600 text-center py-4">{{ error }}</p>

    <!-- Table -->
    <div v-if="!loading && reviews.length" class="overflow-x-auto">
      <table class="min-w-full table-auto border-collapse bg-white rounded-md shadow-md">
        <thead class="bg-gray-100 text-gray-700 font-semibold">
        <tr>
          <th class="px-6 py-3 text-left">Review ID</th>
          <th class="px-6 py-3 text-left">User</th>
          <th class="px-6 py-3 text-left">Product</th>
          <th class="px-6 py-3 text-left">Flags</th>
          <th class="px-6 py-3 text-left">Content</th>
          <th class="px-6 py-3 text-right">Actions</th>
        </tr>
        </thead>
        <tbody class="divide-y divide-gray-200">
        <tr v-for="rev in reviews" :key="rev.id" class="hover:bg-gray-50 transition-colors">
          <td class="px-6 py-4 font-semibold">{{ rev.id }}</td>
          <td class="px-6 py-4 font-medium">{{ rev.user?.name || 'N/A' }}</td>
          <td class="px-6 py-4">{{ rev.product?.name || 'N/A' }}</td>
          <td class="px-6 py-4">
              <span class="inline-block bg-yellow-200 text-yellow-800 text-xs font-semibold px-2 py-1 rounded-full">
                {{ rev.flag_count }}
              </span>
          </td>
          <td class="px-6 py-4">{{ rev.review }}</td>
          <td class="px-6 py-4 flex">
            <button @click="dismissFlag(rev)" class="bg-green-600 hover:bg-green-700 text-white px-4 py-2 rounded-md font-semibold text-sm">
              Dismiss
            </button>
            <button @click="confirmDelete(rev)" class="bg-red-600 hover:bg-red-700 text-white px-4 py-2 rounded-md font-semibold text-sm ml-2">
              Delete
            </button>
          </td>
        </tr>
        </tbody>
      </table>
    </div>
    <p v-else-if="!loading" class="text-gray-500 text-center py-6">No flagged reviews available.</p>

    <!-- Delete Confirmation Modal -->
    <div v-if="confirm.visible" class="fixed inset-0 flex items-center justify-center bg-black bg-opacity-50 z-50">
      <div class="bg-white rounded-lg p-6 shadow-lg w-full max-w-sm">
        <h3 class="text-xl font-semibold mb-4">Confirm Deletion</h3>
        <p class="mb-6">Are you sure you want to delete review #{{ confirm.review.id }}?</p>
        <div class="flex justify-end gap-4">
          <button @click="cancelDelete" class="px-4 py-2 bg-gray-200 rounded-md hover:bg-gray-300">Cancel</button>
          <button @click="deleteReview(confirm.review)" class="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700">Delete</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue';
import axios from 'axios';
import Toast from '@/components/Toast.vue';

const apiBase = `${process.env.VUE_APP_API_URL}/api/v1`;

const reviews = ref([]);
const loading = ref(false);
const error = ref('');

const toasts = reactive([]);
function removeToast(id) {
  const idx = toasts.findIndex(t => t.id === id);
  if (idx !== -1) toasts.splice(idx, 1);
}
function showToast(message, type = 'success') {
  const id = Date.now() + Math.random();
  toasts.push({ id, message, type });
  setTimeout(() => removeToast(id), 5000);
}

const confirm = reactive({ visible: false, review: null });
function confirmDelete(rev) {
  confirm.review = rev;
  confirm.visible = true;
}
function cancelDelete() {
  confirm.visible = false;
  confirm.review = null;
}

async function fetchFlaggedReviews() {
  loading.value = true;
  error.value = '';
  try {
    const { data } = await axios.get(`${apiBase}/flagged-reviews`);
    reviews.value = data.data;
  } catch (err) {
    error.value = err.response?.data?.message || 'Error loading flagged reviews.';
    showToast(error.value, 'error');
  } finally {
    loading.value = false;
  }
}

async function dismissFlag(rev) {
  try {
    await axios.post(`${apiBase}/reset-flag`, { type: 'review', id: rev.id });
    reviews.value = reviews.value.filter(r => r.id !== rev.id);
    showToast('Flag dismissed', 'success');
  } catch (err) {
    console.error(err);
    showToast('Failed to dismiss flag', 'error');
  }
}

async function deleteReview(rev) {
  try {
    await axios.delete(`${apiBase}/admin/reviews/${rev.id}`);
    reviews.value = reviews.value.filter(r => r.id !== rev.id);
    showToast('Review deleted', 'success');
  } catch (err) {
    console.error(err);
    showToast('Failed to delete review', 'error');
  } finally {
    cancelDelete();
  }
}

onMounted(fetchFlaggedReviews);
</script>

<style scoped>
/* fade animations if needed */
</style>
