<template>
  <div class="fixed inset-0 bg-black bg-opacity-50 z-50 overflow-y-auto flex items-center justify-center">
    <!-- Toast stack (teleported to body) -->
    <Toast :toasts="toasts" @close="removeToast" />

    <div class="bg-white rounded-lg shadow-lg p-6 w-full max-w-lg relative max-h-[90vh] overflow-y-auto">
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
      <h2 class="text-2xl font-semibold mb-4">Edit Profile</h2>

      <!-- Profile Image -->
      <div class="mb-4">
        <label class="block font-medium mb-1">Current Profile Picture:</label>
        <img
            v-if="imagePreview"
            :src="imagePreview"
            alt="Profile Preview"
            class="h-20 w-20 rounded-full object-cover mb-2"
        />
        <input type="file" @change="handleImageChange" accept="image/*" />
      </div>

      <!-- Text Fields -->
      <div class="mb-4" v-for="field in textFields" :key="field.name">
        <label :for="field.name" class="block font-medium mb-1">{{ field.label }}</label>
        <input
            :type="field.type"
            :id="field.name"
            v-model="formData[field.name]"
            class="w-full p-2 border rounded focus:outline-none focus:ring-2 focus:ring-brown-500"
        />
      </div>

      <!-- Gender -->
      <div class="mb-4">
        <label class="block font-medium mb-1">Gender:</label>
        <select
            v-model="formData.gender"
            class="w-full p-2 border rounded focus:outline-none focus:ring-2 focus:ring-brown-500"
        >
          <option value="">Select Gender</option>
          <option value="male">Male</option>
          <option value="female">Female</option>
          <option value="other">Other</option>
        </select>
      </div>

      <!-- Date of Birth -->
      <div class="mb-4">
        <label class="block font-medium mb-1">Date of Birth:</label>
        <input
            type="date"
            v-model="formData.date_of_birth"
            class="w-full p-2 border rounded focus:outline-none focus:ring-2 focus:ring-brown-500"
        />
      </div>

      <!-- New Password -->
      <div class="mb-4">
        <label class="block font-medium mb-1">New Password (optional):</label>
        <input v-model="formData.password" type="password" class="input" />
      </div>

      <!-- Confirm Password -->
      <div class="mb-4">
        <label class="block font-medium mb-1">Confirm New Password:</label>
        <input v-model="formData.password_confirmation" type="password" class="input" />
      </div>

      <!-- Existing Password (if they set a new one) -->
      <div v-if="requiresExistingPassword" class="mb-4">
        <label class="block font-medium mb-1">Existing Password:</label>
        <input v-model="formData.existing_password" type="password" class="input" />
      </div>

      <!-- Actions -->
      <div class="flex justify-end space-x-2 mt-4">
        <button
            @click="submitForm"
            :disabled="isSubmitting"
            class="bg-brown-600 text-white px-4 py-2 rounded hover:bg-brown-700 disabled:opacity-50"
        >
          {{ isSubmitting ? 'Updating...' : 'Update Profile' }}
        </button>
<!--        <button @click="closeForm" class="bg-gray-300 text-black px-4 py-2 rounded hover:bg-gray-400">-->
<!--          Cancel-->
<!--        </button>-->
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, reactive, defineProps, defineEmits } from 'vue'
import axios from 'axios'
import Toast from '@/components/Toast.vue'

const props = defineProps({ user: Object })
const emit = defineEmits(['close', 'updated'])

const apiBaseUrl = process.env.VUE_APP_API_URL
const token = localStorage.getItem('authToken')

// Form state
const formData = reactive({
  name: props.user.name || '',
  username: props.user.username || '',
  email: props.user.email || '',
  phone: props.user.phone || '',
  gender: props.user.gender || '',
  date_of_birth: props.user.date_of_birth?.slice(0, 10) || '',
  password: '',
  password_confirmation: '',
  existing_password: ''
})

const textFields = [
  { name: 'name', label: 'Name:', type: 'text' },
  { name: 'username', label: 'Username:', type: 'text' },
  { name: 'email', label: 'Email:', type: 'email' },
  { name: 'phone', label: 'Phone:', type: 'text' }
]

// Toast stack
const toasts = reactive([])
function removeToast(id) {
  const idx = toasts.findIndex(t => t.id === id)
  if (idx !== -1) toasts.splice(idx, 1)
}
function showToast(message, type = 'success') {
  const id = Date.now() + Math.random()
  toasts.push({ id, message, type })
  setTimeout(() => removeToast(id), 5000)
}

const isSubmitting = ref(false)
const imagePreview = ref(props.user.profile_picture || '')
const selectedImage = ref(null)

function handleImageChange(e) {
  const file = e.target.files[0]
  if (file) {
    selectedImage.value = file
    imagePreview.value = URL.createObjectURL(file)
  }
}

const requiresExistingPassword = computed(
    () => formData.password || formData.password_confirmation
)

function closeForm() {
  emit('close')
}

async function submitForm() {
  isSubmitting.value = true
  try {
    const payload = new FormData()
    Object.entries(formData).forEach(([key, val]) => {
      if (val) payload.append(key, val)
    })
    if (selectedImage.value) {
      payload.append('profile_picture', selectedImage.value)
    }

    const { data } = await axios.post(
        `${apiBaseUrl}/api/v1/user/update?_method=PUT`,
        payload,
        {
          headers: {
            Authorization: `Bearer ${token}`,
            'Content-Type': 'multipart/form-data'
          }
        }
    )

    showToast(data.message || 'Profile updated successfully.', 'success')
    emit('updated', data.user)
  } catch (err) {
    const msg = err.response?.data?.message || 'Failed to update profile.'
    showToast(msg, 'error')
    console.error(err)
  } finally {
    isSubmitting.value = false
  }
}
</script>

<style scoped>
.input {
  width: 100%;
  padding: 0.5rem;
  border: 1px solid #cbd5e1;
  border-radius: 0.375rem;
  outline: none;
}
.input:focus {
  border-color: #3b82f6;
}
</style>
