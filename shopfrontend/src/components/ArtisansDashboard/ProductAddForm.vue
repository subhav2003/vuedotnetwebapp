<template>
  <div class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
    <div class="relative bg-[#1a1a1a] text-[#C8B280] rounded-lg shadow-xl w-full max-w-2xl max-h-[90vh] overflow-y-auto p-6">
      <button @click="$emit('close')" class="absolute top-4 right-4 text-[#999] hover:text-red-500 text-2xl">&times;</button>

      <Toast :toasts="toasts" @close="removeToast" />

      <h2 class="text-2xl font-semibold mb-4">Add New Book</h2>
      <form @submit.prevent="addBook" class="space-y-4" enctype="multipart/form-data">

        <!-- Title & Author -->
        <div>
          <label class="block">Title</label>
          <input v-model="book.title" type="text" class="input-field" required />
        </div>
        <div>
          <label class="block">Author</label>
          <input v-model="book.author" type="text" class="input-field" required />
        </div>

        <!-- Genre -->
        <div>
          <label class="block">Genre</label>
          <select v-model.number="book.genreId" class="input-field" required>
            <option value="" disabled>Select a genre</option>
            <option v-for="g in genres" :key="g.id" :value="g.id">
              {{ g.name }}
            </option>
          </select>
        </div>

        <!-- ISBN, Language -->
        <div class="grid grid-cols-2 gap-4">
          <div>
            <label class="block">ISBN</label>
            <input v-model="book.isbn" type="text" class="input-field" />
          </div>
          <div>
            <label class="block">Language</label>
            <input v-model="book.language" type="text" class="input-field" />
          </div>
        </div>

        <!-- Format -->
        <div>
          <label class="block">Format</label>
          <input v-model="book.format" type="text" class="input-field" />
        </div>

        <!-- Description -->
        <div>
          <label class="block">Description</label>
          <textarea v-model="book.description" rows="3" class="input-field" placeholder="Enter book description..."></textarea>
        </div>

        <!-- Publisher & Book Type -->
        <div class="grid grid-cols-2 gap-4">
          <div>
            <label class="block">Publisher</label>
            <input v-model="book.publisher" type="text" class="input-field" />
          </div>
          <div>
            <label class="block">Book Type</label>
            <input v-model="book.bookType" type="text" placeholder="Signed, Limited..." class="input-field" />
          </div>
        </div>

        <!-- Exclusive Edition Toggle -->
        <div>
          <label class="flex items-center">
            <input v-model="book.isExclusiveEdition" type="checkbox" class="mr-2" />
            Exclusive Edition
          </label>
        </div>

        <!-- Price, Stock, Publication Date -->
        <div class="grid grid-cols-3 gap-4">
          <div>
            <label class="block">Price</label>
            <input v-model.number="book.price" type="number" step="0.01" class="input-field" />
          </div>
          <div>
            <label class="block">Stock</label>
            <input v-model.number="book.stock" type="number" min="0" class="input-field" />
          </div>
          <div>
            <label class="block">Publication Date</label>
            <input v-model="book.publicationDate" type="date" class="input-field" />
          </div>
        </div>

        <!-- Toggles -->
        <div class="flex gap-6">
          <label class="flex items-center">
            <input v-model="book.isPhysicalAccess" type="checkbox" class="mr-2" />
            Physical Access
          </label>
          <label class="flex items-center">
            <input v-model="book.isOnSale" type="checkbox" class="mr-2" />
            On Sale
          </label>
        </div>

        <!-- Discounts -->
        <div v-if="book.isOnSale" class="grid grid-cols-3 gap-4">
          <div>
            <label class="block">Discount %</label>
            <input v-model.number="book.discountPercentage" type="number" min="0" max="100" class="input-field" />
          </div>
          <div>
            <label class="block">Start Date</label>
            <input v-model="book.discountStart" type="date" class="input-field" />
          </div>
          <div>
            <label class="block">End Date</label>
            <input v-model="book.discountEnd" type="date" class="input-field" />
          </div>
        </div>

        <!-- Images -->
        <div>
          <label class="block">Images</label>
          <input type="file" accept="image/*" multiple @change="handleImageChange" class="input-field" />
          <div class="flex flex-wrap gap-4 mt-3">
            <div v-for="(img, index) in imagePreviews" :key="index" class="relative w-24 h-24 border rounded overflow-hidden">
              <img :src="img.url" class="object-cover w-full h-full" />
              <button @click="removeImage(index)" type="button" class="absolute top-0 right-0 bg-red-600 text-white px-1 text-xs">&times;</button>
            </div>
          </div>
        </div>

        <!-- Submit -->
        <button
            type="submit"
            :disabled="loading"
            class="w-full bg-blue-700 text-white py-2 rounded-lg hover:bg-blue-600 transition flex items-center justify-center disabled:opacity-50"
        >
          <span v-if="loading" class="loader w-4 h-4 mr-2"></span>
          <span>{{ loading ? 'Adding...' : 'Add Book' }}</span>
        </button>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue';
import axios from 'axios';
import Toast from '@/components/Toast.vue';

const apiBaseUrl = process.env.VUE_APP_API_URL;

const book = reactive({
  title: '', author: '', genreId: null, isbn: '', language: '', format: '',
  description: '', publisher: '', bookType: '', isExclusiveEdition: false,
  price: 0, stock: 0, publicationDate: '', isPhysicalAccess: false,
  isOnSale: false, discountPercentage: 0, discountStart: '', discountEnd: ''
});

const imageFiles = ref([]);
const imagePreviews = ref([]);

function handleImageChange(e) {
  const files = Array.from(e.target.files);
  files.forEach(file => {
    const url = URL.createObjectURL(file);
    imagePreviews.value.push({ url, file });
    imageFiles.value.push(file);
  });
}

function removeImage(index) {
  URL.revokeObjectURL(imagePreviews.value[index].url);
  imagePreviews.value.splice(index, 1);
  imageFiles.value.splice(index, 1);
}

const genres = ref([]);
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

async function fetchGenres() {
  try {
    const { data } = await axios.get(`${apiBaseUrl}/api/Book/genres`);
    genres.value = data.data || data;
  } catch {
    showToast('Failed to load genres', 'error');
  }
}

async function addBook() {
  loading.value = true;
  try {
    const formData = new FormData();
    for (const key in book) {
      formData.append(key, book[key] ?? '');
    }
    imageFiles.value.forEach(file => {
      formData.append('images', file);
    });

    const { data } = await axios.post(`${apiBaseUrl}/api/Book`, formData, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('authToken')}`,
        'Content-Type': 'multipart/form-data'
      }
    });

    showToast(data.message || 'Book added successfully!');
    Object.keys(book).forEach(key => book[key] = (typeof book[key] === 'boolean' ? false : ''));

    book.price = 0;
    book.stock = 0;

    imageFiles.value = [];
    imagePreviews.value.forEach(img => URL.revokeObjectURL(img.url));
    imagePreviews.value = [];

  } catch (err) {
    const msgs = err.response?.data?.errors;
    if (msgs) Object.values(msgs).flat().forEach(m => showToast(m, 'error'));
    else showToast(err.response?.data?.message || 'Add failed', 'error');
  } finally {
    loading.value = false;
  }
}

onMounted(fetchGenres);
</script>

<style scoped>
.input-field {
  width: 100%;
  padding: .5rem;
  background-color: #1f1f1f;
  border: 1px solid #3a3a3a;
  border-radius: .375rem;
  margin-top: .25rem;
  color: #C8B280;
  outline: none;
  transition: border-color .2s, background-color .2s;
}
.input-field:focus {
  border-color: #C8B280;
  background-color: #2a2a2a;
}
.loader {
  border-top: 3px solid #f3f3f3;
  border-right: 3px solid #C8B280;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}
@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>
