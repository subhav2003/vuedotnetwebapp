<template>
  <!-- Full-screen overlay -->
  <div class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
    <!-- Toast stack (teleported to body) -->
    <Toast :toasts="toasts" @close="removeToast" />

    <!-- Modal Container -->
    <div class="relative bg-white w-full max-w-lg mx-auto p-6 rounded-lg shadow-lg overflow-y-auto max-h-[90vh]">
      <!-- Close Button -->
      <button
          @click="closeForm"
          class="absolute top-4 right-4 text-gray-500 hover:text-red-600 transition-colors"
      >
        <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none"
             viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                d="M6 18L18 6M6 6l12 12" />
        </svg>
      </button>

      <h2 class="text-2xl font-bold text-gray-800 mb-6">Initiate Custom Order</h2>

      <!-- Custom Order Form -->
      <form @submit.prevent="submitCustomOrder" class="space-y-4">
        <!-- Order Description -->
        <div>
          <label for="description" class="block text-gray-700 mb-1">Order Description</label>
          <textarea
              id="description"
              v-model="form.description"
              rows="4"
              class="w-full p-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="Describe your custom order requirements..."
          ></textarea>
        </div>

        <!-- Budget -->
        <div>
          <label for="budget" class="block text-gray-700 mb-1">Budget ($)</label>
          <input
              id="budget"
              type="number"
              step="0.01"
              v-model="form.budget"
              class="w-full p-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="e.g., 500.00"
          />
        </div>

        <!-- Deadline (Date Only) -->
        <div>
          <label for="deadline" class="block text-gray-700 mb-1">Deadline (Date)</label>
          <input
              id="deadline"
              type="date"
              v-model="form.deadline"
              class="w-full p-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        <!-- Form Actions -->
        <div class="flex justify-end gap-4 mt-6">
          <button
              type="button"
              @click="closeForm"
              class="bg-gray-300 text-gray-700 py-2 px-4 rounded hover:bg-gray-400"
          >
            Cancel
          </button>
          <button
              type="submit"
              :disabled="loading"
              class="bg-blue-600 text-white py-2 px-4 rounded hover:bg-blue-700 disabled:opacity-50"
          >
            <span v-if="loading">Submitting...</span>
            <span v-else>Submit Order</span>
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue';
import axios from 'axios';
import Toast from '@/components/Toast.vue';

const props = defineProps({
  artisanId: {
    type: Number,
    required: true,
  },
});
const emit = defineEmits(['close', 'order-submitted']);

// Form state
const form = ref({
  description: '',
  budget: '',
  deadline: '',
});

// Loading state
const loading = ref(false);

// Toast stack
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

async function submitCustomOrder() {
  loading.value = true;
  try {
    const token = localStorage.getItem('authToken');
    const payload = {
      artisan_id: props.artisanId,
      description: form.value.description,
      budget: form.value.budget,
      deadline: form.value.deadline,
    };

    const response = await axios.post(
        `${process.env.VUE_APP_API_URL}/api/v1/custom-orders`,
        payload,
        { headers: { Authorization: `Bearer ${token}` } }
    );

    const msg = response.data.message || 'Custom order submitted successfully.';
    showToast(msg, 'success');

    // ► CLEAR THE FORM HERE ◄
    form.value = {
      description: '',
      budget: '',
      deadline: '',
    };

    // Emit the new order data back to parent
    emit('order-submitted', response.data);
  } catch (error) {
    console.error('Error submitting custom order:', error);
    const errMsg = error.response?.data?.message || 'Failed to submit custom order.';
    showToast(errMsg, 'error');
  } finally {
    loading.value = false;
  }
}

function closeForm() {
  emit('close');
}
</script>

<style scoped>
/* No changes needed here */
</style>
