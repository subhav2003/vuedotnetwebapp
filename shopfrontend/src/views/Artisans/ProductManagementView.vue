<template>
  <div class="min-h-screen flex flex-col bg-gray-900 text-[#C8B280]">
    <!-- Artisan Sidebar / Navbar -->
    <ArtisanNavBar />

    <!-- Toasts -->
    <Toast :toasts="toasts" @close="removeToast" />

    <main class="flex-1 p-6">
      <!-- Header & Add Buttons -->
      <div class="flex justify-between items-center mb-8">
        <h1 class="text-3xl font-semibold">Product Management</h1>
        <div class="space-x-4">
          <button
              @click="showGenreForm = true"
              class="bg-green-700 text-white px-4 py-2 rounded hover:bg-green-800"
          >
            Add Genre
          </button>
          <button
              @click="showBookForm = true"
              class="bg-blue-700 text-white px-4 py-2 rounded hover:bg-blue-800"
          >
            Add Book
          </button>
        </div>
      </div>

      <!-- Genres Table -->
      <section class="mb-12">
        <h2 class="text-2xl mb-4">Genres</h2>
        <div v-if="loadingGenres" class="flex justify-center py-12">
          <div class="loader ease-linear rounded-full border-4 border-t-4 border-[#C8B280] h-12 w-12"></div>
        </div>
        <div v-else class="overflow-x-auto rounded-lg shadow border border-[#3a3a3a]">
          <table class="min-w-full table-auto text-sm bg-[#1a1a1a] text-[#C8B280]">
            <thead class="bg-[#2a2a2a]">
            <tr>
              <th class="px-4 py-2 text-left">ID</th>
              <th class="px-4 py-2 text-left">Name</th>
              <th class="px-4 py-2 text-left">Actions</th>
            </tr>
            </thead>
            <tbody>
            <tr
                v-for="g in genres"
                :key="g.id"
                class="hover:bg-[#333333] border-t border-[#3a3a3a]"
            >
              <td class="px-4 py-2">{{ g.id }}</td>
              <td class="px-4 py-2">{{ g.name }}</td>
              <td class="px-4 py-2">
                <button
                    @click="openDeleteGenreModal(g.id)"
                    class="bg-red-700 px-3 py-1 rounded text-white hover:bg-red-800"
                >
                  Delete
                </button>
              </td>
            </tr>
            <tr v-if="!genres.length">
              <td colspan="3" class="px-4 py-4 text-center text-gray-500 italic">
                No genres found.
              </td>
            </tr>
            </tbody>
          </table>
        </div>
      </section>

      <!-- Books Table -->
      <section class="mb-12">
        <h2 class="text-2xl mb-4">Books</h2>
        <div v-if="loadingBooks" class="flex justify-center py-12">
          <div class="loader ease-linear rounded-full border-4 border-t-4 border-[#C8B280] h-12 w-12"></div>
        </div>
        <div v-else class="overflow-x-auto rounded-lg shadow border border-[#3a3a3a]">
          <table class="min-w-full table-auto text-sm bg-[#1a1a1a] text-[#C8B280]">
            <thead class="bg-[#2a2a2a]">
            <tr>
              <th class="px-4 py-2 text-left">ID</th>
              <th class="px-4 py-2 text-left">Title</th>
              <th class="px-4 py-2 text-left">Genre</th>
              <th class="px-4 py-2 text-left">Price</th>
              <th class="px-4 py-2 text-left">Stock</th>
              <th class="px-4 py-2 text-left">Actions</th>
            </tr>
            </thead>
            <tbody>
            <tr
                v-for="b in books"
                :key="b.id"
                class="hover:bg-[#333333] border-t border-[#3a3a3a]"
            >
              <td class="px-4 py-2">{{ b.id }}</td>
              <td class="px-4 py-2">{{ b.title }}</td>
              <td class="px-4 py-2">{{ b.genreName }}</td>
              <td class="px-4 py-2">${{ b.price.toFixed(2) }}</td>
              <td class="px-4 py-2">{{ b.stock }}</td>
              <td class="px-4 py-2 space-x-2">
                <button
                    @click="openEditBookModal(b.id)"
                    class="bg-yellow-600 text-black px-3 py-1 rounded hover:bg-yellow-700"
                >
                  Edit
                </button>
                <button
                    @click="openDeleteBookModal(b.id)"
                    class="bg-red-600 text-white px-3 py-1 rounded hover:bg-red-700"
                >
                  Delete
                </button>
              </td>
            </tr>
            <tr v-if="!books.length">
              <td colspan="6" class="px-4 py-4 text-center text-gray-500 italic">
                No books found.
              </td>
            </tr>
            </tbody>
          </table>
        </div>
      </section>

      <!-- Preview Grid (just like shop view) -->
      <section class="py-10 px-4 sm:px-8 bg-gray-900">
        <h2 class="text-2xl mb-4 text-center">Preview Books</h2>
        <div v-if="loadingBooks" class="flex justify-center items-center h-64">
          <div class="loader ease-linear rounded-full border-4 border-t-4 border-[#C8B280] h-12 w-12"></div>
        </div>
        <div v-else-if="!books.length" class="text-center text-gray-500 italic">
          No books to preview.
        </div>
        <div v-else class="grid grid-cols-2 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
          <ProductCard
              v-for="book in books"
              :key="book.id"
              :book="book"
              @showBookDetail="openProductDetail"
          >
            <template #action-button>
              <button
                  @click="openEditBookModal(book.id)"
                  class="bg-yellow-600 text-black px-3 py-1 rounded mr-2"
              >
                Edit
              </button>
              <button
                  @click="openDeleteBookModal(book.id)"
                  class="bg-red-600 text-white px-3 py-1 rounded"
              >
                Delete
              </button>
            </template>
          </ProductCard>
        </div>
      </section>

      <!-- ProductDetail modal exactly as in shop view -->
      <ProductDetail
          v-if="showProductDetail"
          :book="selectedBook"
          @close="closeProductDetail"
      />
    </main>

    <Footer />

    <!-- Add / Edit Forms -->
    <AddGenreForm
        v-if="showGenreForm"
        @created="onGenreCreated"
        @close="() => showGenreForm = false"
    />
    <ProductAddForm
        v-if="showBookForm"
        @created="onBookCreated"
        @close="() => showBookForm = false"
    />
    <ProductEditForm
        v-if="showEditForm"
        :book-id="bookToEditId"
        @updated="onBookUpdated"
        @close="() => { showEditForm = false; bookToEditId = null }"
    />

    <!-- Inline Delete Confirmation for Genre -->
    <div
        v-if="showDeleteGenreModal"
        class="fixed inset-0 bg-black bg-opacity-60 flex items-center justify-center z-50"
    >
      <div class="bg-[#1a1a1a] p-6 rounded-lg shadow-lg text-[#C8B280] max-w-sm w-full">
        <h3 class="text-xl font-semibold mb-4">Delete Genre?</h3>
        <p class="mb-6">This will remove the genre permanently. Continue?</p>
        <div class="flex justify-end space-x-4">
          <button @click="showDeleteGenreModal = false" class="px-4 py-2 bg-gray-500 rounded">
            Cancel
          </button>
          <button
              @click="confirmDeleteGenre"
              class="px-4 py-2 bg-red-700 text-white rounded"
          >
            Delete
          </button>
        </div>
      </div>
    </div>

    <!-- Inline Delete Confirmation for Book -->
    <div
        v-if="showDeleteBookModal"
        class="fixed inset-0 bg-black bg-opacity-60 flex items-center justify-center z-50"
    >
      <div class="bg-[#1a1a1a] p-6 rounded-lg shadow-lg text-[#C8B280] max-w-sm w-full">
        <h3 class="text-xl font-semibold mb-4">Delete Book?</h3>
        <p class="mb-6">This will remove the book permanently. Continue?</p>
        <div class="flex justify-end space-x-4">
          <button @click="showDeleteBookModal = false" class="px-4 py-2 bg-gray-500 rounded">
            Cancel
          </button>
          <button
              @click="confirmDeleteBook"
              class="px-4 py-2 bg-red-700 text-white rounded"
          >
            Delete
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue';
import axios from 'axios';

import ArtisanNavBar    from '@/components/ArtisansDashboard/ArtisanNavBar.vue';
import Toast            from '@/components/Toast.vue';
import ProductCard      from '@/components/ProductCard.vue';
import ProductDetail    from '@/components/ProductDetail.vue';
import AddGenreForm     from '@/components/ArtisansDashboard/AddGenreForm.vue';
import ProductAddForm   from '@/components/ArtisansDashboard/ProductAddForm.vue';
import ProductEditForm  from '@/components/ArtisansDashboard/ProductEditForm.vue';
import Footer           from '@/components/Footer.vue';

const api = process.env.VUE_APP_API_URL;
const toasts = reactive([]);

function showToast(message, type = 'success') {
  const id = Date.now() + Math.random();
  toasts.push({ id, message, type });
  setTimeout(() => {
    const i = toasts.findIndex(t => t.id === id);
    if (i !== -1) toasts.splice(i, 1);
  }, 5000);
}

// State
const genres           = ref([]);
const books            = ref([]);
const loadingGenres    = ref(false);
const loadingBooks     = ref(false);

const showGenreForm        = ref(false);
const showBookForm         = ref(false);
const showEditForm         = ref(false);
const bookToEditId         = ref(null);

const showDeleteGenreModal = ref(false);
const genreToDelete        = ref(null);

const showDeleteBookModal  = ref(false);
const bookToDelete         = ref(null);

const showProductDetail    = ref(false);
const selectedBook         = ref(null);

// Fetchers
async function fetchGenres() {
  loadingGenres.value = true;
  try {
    const { data } = await axios.get(`${api}/api/Book/genres`);
    genres.value = data.data || data;
  } catch {
    showToast('Failed to load genres', 'error');
  } finally {
    loadingGenres.value = false;
  }
}

async function fetchBooks() {
  loadingBooks.value = true;
  try {
    const { data } = await axios.get(`${api}/api/Book`);
    books.value = (data.data || data).map(b => ({
      ...b,
      genreName: b.genreName || b.category?.name || '—'
    }));
  } catch {
    showToast('Failed to load books', 'error');
  } finally {
    loadingBooks.value = false;
  }
}

// Callbacks
function onGenreCreated() {
  showGenreForm.value = false;
  fetchGenres();
  showToast('Genre added!');
}

function onBookCreated() {
  showBookForm.value = false;
  fetchBooks();
  showToast('Book added!');
}

function onBookUpdated() {
  showEditForm.value = false;
  bookToEditId.value = null;
  fetchBooks();
  showToast('Book updated!');
}

// Edit modal
function openEditBookModal(id) {
  bookToEditId.value = id;
  showEditForm.value = true;
}

// Delete genre
function openDeleteGenreModal(id) {
  genreToDelete.value = id;
  showDeleteGenreModal.value = true;
}
async function confirmDeleteGenre() {
  try {
    await axios.delete(`${api}/api/Book/genres/${genreToDelete.value}`);
    showToast('Genre deleted.');
    fetchGenres();
  } catch {
    showToast('Failed to delete genre', 'error');
  } finally {
    showDeleteGenreModal.value = false;
    genreToDelete.value = null;
  }
}

// Delete book
function openDeleteBookModal(id) {
  bookToDelete.value = id;
  showDeleteBookModal.value = true;
}
async function confirmDeleteBook() {
  try {
    await axios.delete(`${api}/api/Book/${bookToDelete.value}`, {
      headers: { Authorization: `Bearer ${localStorage.getItem('authToken')}` }
    });
    showToast('Book deleted.');
    fetchBooks();
  } catch {
    showToast('Failed to delete book', 'error');
  } finally {
    showDeleteBookModal.value = false;
    bookToDelete.value = null;
  }
}

// Detail modal
function openProductDetail(book) {
  selectedBook.value = book;
  showProductDetail.value = true;
  document.body.style.overflow = 'hidden';
}
function closeProductDetail() {
  showProductDetail.value = false;
  selectedBook.value = null;
  document.body.style.overflow = '';
}

onMounted(() => {
  fetchGenres();
  fetchBooks();
});
</script>

<style scoped>
.loader {
  border-top-color: #C8B280;
  animation: spin 1s linear infinite;
}
@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>
