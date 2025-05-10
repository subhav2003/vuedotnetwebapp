<template>
  <div v-if="loaded" class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
    <div class="relative bg-[#1a1a1a] text-[#C8B280] rounded-lg shadow-xl w-full max-w-2xl max-h-[90vh] overflow-y-auto p-6">
      <button @click="closeForm" class="absolute top-4 right-4 text-[#aaa] hover:text-red-600 text-2xl">&times;</button>

      <Toast :toasts="toasts" @close="removeToast" />
      <h2 class="text-2xl font-semibold mb-4">Edit Book</h2>

      <form @submit.prevent="updateBook" class="space-y-4" enctype="multipart/form-data">
        <div>
          <label class="block">Title</label>
          <input v-model="book.title" type="text" class="input-field" required />
        </div>
        <div>
          <label class="block">Author</label>
          <input v-model="book.author" type="text" class="input-field" required />
        </div>
        <div>
          <label class="block">Genre</label>
          <select v-model.number="book.genreId" class="input-field" required>
            <option disabled value="">Select genre</option>
            <option v-for="g in genres" :key="g.id" :value="g.id">{{ g.name }}</option>
          </select>
        </div>
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
        <div>
          <label class="block">Format</label>
          <input v-model="book.format" type="text" class="input-field" />
        </div>
        <div>
          <label class="block">Publisher</label>
          <input v-model="book.publisher" type="text" class="input-field" />
        </div>
        <div>
          <label class="block">Book Type</label>
          <input v-model="book.bookType" type="text" class="input-field" />
        </div>
        <div>
          <label class="block">Description</label>
          <textarea v-model="book.description" class="input-field" rows="3" />
        </div>

        <div class="grid grid-cols-3 gap-4">
          <div>
            <label class="block">Price</label>
            <input v-model.number="book.price" type="number" class="input-field" />
          </div>
          <div>
            <label class="block">Stock</label>
            <input v-model.number="book.stock" type="number" class="input-field" />
          </div>
          <div>
            <label class="block">Publication Date</label>
            <input v-model="book.publicationDate" type="date" class="input-field" />
          </div>
        </div>

        <div class="flex gap-6">
          <label class="flex items-center">
            <input v-model="book.isPhysicalAccess" type="checkbox" class="mr-2" />
            Physical Access
          </label>
          <label class="flex items-center">
            <input v-model="book.isOnSale" type="checkbox" class="mr-2" />
            On Sale
          </label>
          <label class="flex items-center">
            <input v-model="book.isExclusiveEdition" type="checkbox" class="mr-2" />
            Exclusive Edition
          </label>
        </div>

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

        <!-- Existing Images -->
        <div v-if="book.images?.length">
          <label class="block">Existing Images</label>
          <div class="flex flex-wrap gap-4 mt-2">
            <div v-for="(img, i) in book.images" :key="i" class="relative w-24 h-24 border rounded overflow-hidden">
              <img :src="apiBaseUrl + img" class="object-cover w-full h-full" />
              <button @click="removeExistingImage(i)" class="absolute top-0 right-0 bg-red-600 text-white px-1 text-xs">&times;</button>
            </div>
          </div>
        </div>

        <!-- New Images -->
        <div>
          <label class="block mt-4">Add New Images</label>
          <input type="file" accept="image/*" multiple @change="handleImageChange" class="input-field" />
          <div class="flex flex-wrap gap-4 mt-3">
            <div v-for="(img, i) in newImagePreviews" :key="i" class="relative w-24 h-24 border rounded overflow-hidden">
              <img :src="img.url" class="object-cover w-full h-full" />
              <button @click="removeNewImage(i)" class="absolute top-0 right-0 bg-red-600 text-white px-1 text-xs">&times;</button>
            </div>
          </div>
        </div>

        <div class="flex justify-end mt-6 space-x-4">
          <button @click="closeForm" type="button" class="bg-gray-600 text-white py-2 px-4 rounded">Cancel</button>
          <button :disabled="loading" type="submit" class="bg-blue-700 text-white py-2 px-6 rounded hover:bg-blue-600 transition disabled:opacity-50">
            <span v-if="loading" class="loader w-4 h-4 mr-2"></span>
            <span>{{ loading ? 'Updating…' : 'Update Book' }}</span>
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue';
import axios from 'axios';
import Toast from '@/components/Toast.vue';

const props = defineProps({ bookId: Number });
const emit = defineEmits(['close', 'updated']);
const apiBaseUrl = process.env.VUE_APP_API_URL;

const loaded = ref(false);
const loading = ref(false);
const genres = ref([]);
const deleteImages = ref([]);
const newImages = ref([]);
const newImagePreviews = ref([]);

const book = reactive({
  id: 0, title: '', author: '', genreId: null, isbn: '',
  language: '', format: '', price: 0, stock: 0, publicationDate: '',
  isPhysicalAccess: false, isOnSale: false, discountPercentage: 0,
  discountStart: '', discountEnd: '', isExclusiveEdition: false,
  description: '', publisher: '', bookType: '', images: []
});

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

async function fetchBook() {
  try {
    const { data } = await axios.get(`${apiBaseUrl}/api/Book/${props.bookId}`);
    const bookData = data.data || data;
    const genreMatch = genres.value.find(g => g.name === bookData.genreName);

    book.id = bookData.id;
    book.title = bookData.title;
    book.author = bookData.author;
    book.genreId = genreMatch?.id || null;
    book.isbn = bookData.isbn;
    book.language = bookData.language;
    book.format = bookData.format;
    book.publisher = bookData.publisher;
    book.bookType = bookData.bookType;
    book.description = bookData.description;
    book.price = bookData.price;
    book.stock = bookData.stock;
    book.publicationDate = bookData.publicationDate?.substring(0, 10) || '';
    book.isPhysicalAccess = bookData.isPhysicalAccess;
    book.isOnSale = bookData.isOnSale;
    book.discountPercentage = bookData.discountPercentage;
    book.discountStart = bookData.discountStart?.substring(0, 10) || '';
    book.discountEnd = bookData.discountEnd?.substring(0, 10) || '';
    book.isExclusiveEdition = bookData.isExclusiveEdition;
    book.images = bookData.images || [];

    loaded.value = true;
  } catch {
    showToast('Failed to load book', 'error');
  }
}


function handleImageChange(e) {
  const files = Array.from(e.target.files);
  files.forEach(file => {
    const url = URL.createObjectURL(file);
    newImagePreviews.value.push({ url, file });
    newImages.value.push(file);
  });
}
function removeNewImage(index) {
  URL.revokeObjectURL(newImagePreviews.value[index].url);
  newImagePreviews.value.splice(index, 1);
  newImages.value.splice(index, 1);
}
function removeExistingImage(index) {
  deleteImages.value.push(book.images[index]);
  book.images.splice(index, 1);
}

async function updateBook() {
  loading.value = true;
  const formData = new FormData();

  formData.append('id', props.bookId);
  for (const key in book) {
    if (key !== 'images') formData.append(key, book[key] ?? '');
  }

  newImages.value.forEach(file => formData.append('images', file));
  deleteImages.value.forEach(img => formData.append('deleteImages[]', img));

  try {
    await axios.put(`${apiBaseUrl}/api/Book/${props.bookId}`, formData, {
      headers: {
        Authorization: `Bearer ${localStorage.getItem('authToken')}`,
        'Content-Type': 'multipart/form-data'
      },
    });
    showToast('Book updated successfully!', 'success');
    emit('updated');
  } catch (err) {
    const msgs = err.response?.data?.errors;
    if (msgs) Object.values(msgs).flat().forEach(m => showToast(m, 'error'));
    else showToast(err.response?.data?.message || 'Update failed', 'error');
  } finally {
    loading.value = false;
  }
}

function closeForm() {
  emit('close');
}

onMounted(async () => {
  await fetchGenres();
  await fetchBook();
});
</script>

<style scoped>
.input-field {
  width: 100%;
  padding: .5rem;
  background-color: #2a2a2a;
  color: #C8B280;
  border: 1px solid #444;
  border-radius: .375rem;
  margin-top: .25rem;
  outline: none;
  transition: border-color .2s;
}
.input-field:focus {
  border-color: #10b981;
  background-color: #333;
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