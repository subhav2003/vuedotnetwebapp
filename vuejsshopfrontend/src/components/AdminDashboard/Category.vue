<template>
  <div class="p-6">
    <!-- Toasts -->
    <Toast :toasts="toasts" @close="removeToast" />

    <!-- Header -->
    <div class="flex justify-between items-center mb-4">
      <h2 class="text-2xl font-semibold text-gray-800">Categories</h2>
      <button
          @click="openForm()"
          class="bg-green-500 text-white px-4 py-2 rounded hover:bg-green600 transition"
      >
        Add Category
      </button>
    </div>

    <!-- Table Card -->
    <div class="bg-white rounded-lg shadow overflow-x-auto">
      <table class="min-w-full divide-y divide-gray-200">
        <thead class="bg-gray-50">
        <tr>
          <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
          <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Description</th>
          <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
        </tr>
        </thead>
        <tbody class="bg-white divide-y divide-gray-200">
        <tr v-for="cat in categories" :key="cat.id" class="hover:bg-gray-50">
          <td class="px-6 py-4 whitespace-nowrap text-gray-800">{{ cat.name }}</td>
          <td class="px-6 py-4 whitespace-nowrap text-gray-600">{{ cat.description || '-' }}</td>
          <td class="px-6 py-4 whitespace-nowrap text-right">
            <button
                @click="openForm(cat)"
                class="text-blue-600 hover:underline mr-4"
            >
              Edit
            </button>
            <button
                @click="confirmDelete(cat.id)"
                class="text-red-600 hover:underline"
            >
              Delete
            </button>
          </td>
        </tr>
        <tr v-if="!categories.length">
          <td colspan="3" class="px-6 py-4 text-center text-gray-500">No categories found.</td>
        </tr>
        </tbody>
      </table>
    </div>

    <!-- Category Form Modal -->
    <div
        v-if="showForm"
        class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50"
    >
      <div class="bg-white rounded-lg shadow-lg w-full max-w-md p-6 relative">
        <button @click="closeForm" class="absolute top-3 right-3 text-gray-500 hover:text-red-600 text-2xl">&times;</button>
        <h3 class="text-xl font-semibold mb-4">{{ form.id ? 'Edit Category' : 'Add Category' }}</h3>
        <form @submit.prevent="submitCategory" class="space-y-4">
          <div>
            <label class="block text-gray-700">Name</label>
            <input v-model="form.name" type="text" class="input-field" required />
          </div>
          <div>
            <label class="block text-gray-700">Description</label>
            <textarea v-model="form.description" rows="3" class="input-field"></textarea>
          </div>
          <div class="flex justify-end space-x-4">
            <button type="button" @click="closeForm" class="px-4 py-2 bg-gray-200 rounded hover:bg-gray-300">Cancel</button>
            <button type="submit" :disabled="loading" class="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700">
              {{ loading ? 'Saving...' : form.id ? 'Update' : 'Create' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue';
import axios from 'axios';
import Toast from '@/components/Toast.vue';

const apiBaseUrl = process.env.VUE_APP_API_URL;

// State
const categories = ref([]);
const loading = ref(false);
const toasts = reactive([]);
const showForm = ref(false);
const form = reactive({ id: null, name: '', description: '' });

// Toast helpers
function removeToast(id) {
  const idx = toasts.findIndex(t => t.id === id);
  if (idx !== -1) toasts.splice(idx, 1);
}
function showToast(message, type = 'success') {
  const id = Date.now() + Math.random();
  toasts.push({ id, message, type });
  setTimeout(() => removeToast(id), 5000);
}

// Fetch categories
async function fetchCategories() {
  try {
    const { data } = await axios.get(`${apiBaseUrl}/api/v1/categories`);
    categories.value = data.data || [];
  } catch {
    showToast('Failed to load categories', 'error');
  }
}

// Open form for add or edit
function openForm(cat = null) {
  if (cat) {
    form.id = cat.id;
    form.name = cat.name;
    form.description = cat.description;
  } else {
    form.id = null;
    form.name = '';
    form.description = '';
  }
  showForm.value = true;
}
function closeForm() {
  showForm.value = false;
}

// Create or update
async function submitCategory() {
  if (!form.name.trim()) return showToast('Name is required', 'error');
  loading.value = true;
  try {
    if (form.id) {
      await axios.put(
          `${apiBaseUrl}/api/v1/categories/${form.id}`,
          { name: form.name, description: form.description }
      );
      showToast('Category updated', 'success');
    } else {
      await axios.post(
          `${apiBaseUrl}/api/v1/categories`,
          { name: form.name, description: form.description }
      );
      showToast('Category created', 'success');
    }
    await fetchCategories();
    closeForm();
  } catch (err) {
    showToast(err.response?.data?.message || 'Operation failed', 'error');
  } finally {
    loading.value = false;
  }
}

// Confirm delete
function confirmDelete(id) {
  if (!confirm('Delete this category?')) return;
  deleteCategory(id);
}
async function deleteCategory(id) {
  try {
    await axios.delete(`${apiBaseUrl}/api/v1/categories/${id}`);
    showToast('Category deleted', 'success');
    fetchCategories();
  } catch {
    showToast('Failed to delete', 'error');
  }
}

// Initial load
onMounted(fetchCategories);
</script>

<style scoped>
.input-field {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid #cbd5e1;
  border-radius: 0.375rem;
  margin-top: 0.25rem;
  outline: none;
  transition: border-color 0.2s;
}
.input-field:focus {
  border-color: #3b82f6;
}
.bg-green600:hover { background-color: #059669; }
.loader { border-top-color: #A0522D; animation: spin 1s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
</style>
