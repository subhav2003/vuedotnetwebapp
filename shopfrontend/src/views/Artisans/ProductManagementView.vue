<template>
  <div class="min-h-screen flex flex-col bg-[#0f0f0f] text-[#C8B280]">
    <ArtisanNavBar />

    <main class="pt-20 px-4 md:px-8 flex-1">
      <Toast :toasts="toasts" @close="removeToast" />

      <!-- Top Bar -->
      <div class="flex justify-between items-center mb-8">
        <h1 class="text-3xl font-semibold">Your Books</h1>
        <div class="space-x-4">
          <button @click="toggleGenreForm" class="bg-green-700 text-white px-4 py-2 rounded hover:bg-green-800">Add Genre</button>
          <button @click="toggleBookForm" class="bg-blue-700 text-white px-4 py-2 rounded hover:bg-blue-800">Add Book</button>
        </div>
      </div>

      <!-- Genre Table -->
      <div class="mb-10">
        <h2 class="text-2xl mb-2">Genres</h2>
        <div v-if="Array.isArray(genres) && genres.length" class="overflow-x-auto rounded-lg shadow border border-[#C8B280]">
          <table class="min-w-full table-auto text-sm bg-[#1a1a1a] text-[#C8B280]">
            <thead class="bg-[#2a2a2a]">
            <tr>
              <th class="px-4 py-2 text-left">ID</th>
              <th class="px-4 py-2 text-left">Name</th>
              <th class="px-4 py-2 text-left">Actions</th>
            </tr>
            </thead>
            <tbody>
            <tr v-for="genre in genres" :key="genre.id" class="hover:bg-[#333333] border-t border-[#3a3a3a]">
              <td class="px-4 py-2">{{ genre.id }}</td>
              <td class="px-4 py-2">{{ genre.name }}</td>
              <td class="px-4 py-2">
                <button @click="deleteGenre(genre.id)" class="bg-red-700 px-3 py-1 rounded text-white hover:bg-red-800">Delete</button>
              </td>
            </tr>
            </tbody>
          </table>
        </div>
        <p v-else class="text-sm text-gray-400 italic">No genres found.</p>
      </div>

      <!-- Book Cards -->
      <div v-if="!isLoading && Array.isArray(books) && books.length" class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
        <ProductCard
            v-for="book in books"
            :key="book.id"
            :book="book"
            @showBookDetail="openBookDetail"
        >
          <template #action-button>
            <button @click="openEditForm(book.id)" class="bg-yellow-600 text-black px-3 py-1 rounded mr-2">Edit</button>
            <button @click="openDeleteModal(book.id)" class="bg-red-600 text-white px-3 py-1 rounded">Delete</button>
          </template>
        </ProductCard>
      </div>

      <div v-else-if="!isLoading && Array.isArray(books) && books.length === 0" class="text-center text-gray-500 py-8">
        No books found.
      </div>

      <div v-if="isLoading" class="text-center py-12 text-[#C8B280]">Loading books...</div>
      <div v-if="errorMessage" class="text-center text-red-400">{{ errorMessage }}</div>

      <!-- Add Book Modal -->
      <div v-if="showBookForm" class="fixed inset-0 bg-black bg-opacity-60 flex justify-center items-center z-50">
        <div class="bg-[#1a1a1a] rounded-lg shadow-lg max-w-md w-full p-6 relative text-[#C8B280]">
          <button @click="toggleBookForm" class="absolute top-2 right-4 text-2xl text-gray-400 hover:text-red-500">×</button>
          <ProductAddForm @created="onBookCreated" @close="toggleBookForm" />
        </div>
      </div>

      <!-- Edit Book Modal -->
      <div v-if="showEditForm && bookToEdit" class="fixed inset-0 bg-black bg-opacity-60 flex justify-center items-center z-50">
        <div class="bg-[#1a1a1a] rounded-lg shadow-lg max-w-md w-full p-6 relative text-[#C8B280]">
          <button @click="closeEditForm" class="absolute top-2 right-4 text-2xl text-gray-400 hover:text-red-500">×</button>
          <ProductEditForm :book-id="bookToEdit.id" @close="closeEditForm" @updated="fetchBooks" />
        </div>
      </div>

      <!-- Add Genre Modal -->
      <div v-if="showGenreForm" class="fixed inset-0 bg-black bg-opacity-60 flex justify-center items-center z-50">
        <div class="bg-[#1a1a1a] rounded-lg shadow-lg max-w-md w-full p-6 relative text-[#C8B280]">
          <button @click="toggleGenreForm" class="absolute top-2 right-4 text-2xl text-gray-400 hover:text-red-500">×</button>
          <AddGenreForm @created="onGenreCreated" @close="toggleGenreForm" />
        </div>
      </div>

      <!-- Delete Confirmation Modal -->
      <div v-if="showDeleteModal" class="fixed inset-0 bg-black bg-opacity-60 flex justify-center items-center z-50">
        <div class="bg-[#1a1a1a] text-[#C8B280] p-6 rounded-lg shadow-lg max-w-sm w-full">
          <h3 class="text-xl font-semibold mb-4">Confirm Deletion</h3>
          <p class="mb-6">Are you sure you want to delete this book?</p>
          <div class="flex justify-end space-x-4">
            <button @click="closeDeleteModal" class="px-4 py-2 bg-gray-500 rounded">Cancel</button>
            <button @click="deleteBook" :disabled="isDeleting" class="px-4 py-2 bg-red-700 text-white rounded">
              {{ isDeleting ? 'Deleting...' : 'Delete' }}
            </button>
          </div>
        </div>
      </div>

      <!-- Book Detail Overlay -->
      <ProductDetail v-if="showBookDetail" :product="selectedBookDetail" @close="closeBookDetail" />
    </main>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue';
import axios from 'axios';

import ArtisanNavBar from '@/components/ArtisansDashboard/ArtisanNavBar.vue';
import ProductCard from '@/components/ProductCard.vue';
import ProductAddForm from '@/components/ArtisansDashboard/ProductAddForm.vue';
import ProductEditForm from '@/components/ArtisansDashboard/ProductEditForm.vue';
import ProductDetail from '@/components/ProductDetail.vue';
import AddGenreForm from '@/components/ArtisansDashboard/AddGenreForm.vue';
import Toast from '@/components/Toast.vue';

const apiBaseUrl = process.env.VUE_APP_API_URL;

const books = ref([]);
const genres = ref([]);
const isLoading = ref(true);
const errorMessage = ref('');

const showBookForm = ref(false);
const showEditForm = ref(false);
const showGenreForm = ref(false);
const bookToEdit = ref(null);

const showDeleteModal = ref(false);
const deleteId = ref(null);
const isDeleting = ref(false);

const showBookDetail = ref(false);
const selectedBookDetail = ref(null);

// Toast logic
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

// Fetching data
async function fetchBooks() {
  isLoading.value = true;
  try {
    const res = await axios.get(`${apiBaseUrl}/api/Book`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('authToken')}` }
    });
    books.value = res.data.data || res.data;
  } catch {
    errorMessage.value = 'Failed to load books.';
  } finally {
    isLoading.value = false;
  }
}

async function fetchGenres() {
  try {
    const res = await axios.get(`${apiBaseUrl}/api/Book/genres`);
    genres.value = res.data.data || res.data;
  } catch {
    showToast('Failed to load genres', 'error');
  }
}

// Modal Actions
function toggleBookForm() { showBookForm.value = !showBookForm.value; }
function toggleGenreForm() { showGenreForm.value = !showGenreForm.value; }

function onBookCreated() {
  toggleBookForm();
  fetchBooks();
  showToast('Book added!');
}

function onGenreCreated() {
  toggleGenreForm();
  fetchGenres();
  showToast('Genre added!');
}

// Book Edit/Delete
function openEditForm(id) {
  bookToEdit.value = books.value.find(b => b.id === id);
  showEditForm.value = true;
}
function closeEditForm() {
  showEditForm.value = false;
  bookToEdit.value = null;
}

function openDeleteModal(id) {
  deleteId.value = id;
  showDeleteModal.value = true;
}
function closeDeleteModal() {
  deleteId.value = null;
  showDeleteModal.value = false;
}

async function deleteBook() {
  if (!deleteId.value) return;
  isDeleting.value = true;
  try {
    await axios.delete(`${apiBaseUrl}/api/Book/${deleteId.value}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('authToken')}` }
    });
    showToast('Book deleted.');
    fetchBooks();
  } catch {
    showToast('Failed to delete.', 'error');
  } finally {
    isDeleting.value = false;
    closeDeleteModal();
  }
}

async function deleteGenre(id) {
  try {
    await axios.delete(`${apiBaseUrl}/api/Book/genres/${id}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('authToken')}` }
    });
    showToast('Genre deleted');
    fetchGenres();
  } catch {
    showToast('Failed to delete genre', 'error');
  }
}

// Book detail modal
function openBookDetail(book) {
  selectedBookDetail.value = book;
  showBookDetail.value = true;
  document.body.style.overflow = 'hidden';
}
function closeBookDetail() {
  selectedBookDetail.value = null;
  showBookDetail.value = false;
  document.body.style.overflow = '';
}

onMounted(() => {
  fetchBooks();
  fetchGenres();
});
</script>

<style scoped>
body {
  font-family: system-ui, sans-serif;
}
</style>
