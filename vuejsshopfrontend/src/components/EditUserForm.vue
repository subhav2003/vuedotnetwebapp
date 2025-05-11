<template>
  <div class="fixed inset-0 bg-black bg-opacity-50 z-50 flex items-center justify-center">
    <Toast :toasts="toasts" @close="removeToast" />
    <div class="bg-white p-6 rounded-lg shadow-lg w-full max-w-lg overflow-auto max-h-[90vh] relative">
      <button @click="$emit('close')" class="absolute top-4 right-4 text-gray-500 hover:text-red-600">
        ✕
      </button>
      <h2 class="text-2xl font-semibold mb-4">Edit Profile</h2>

      <form @submit.prevent="save" class="space-y-4">
        <div v-for="f in fields" :key="f.name">
          <label :for="f.name" class="block font-medium mb-1">{{ f.label }}</label>
          <input
              :id="f.name"
              :type="f.type"
              v-model="form[f.name]"
              class="w-full p-2 border rounded"
              required
          />
        </div>

        <div>
          <label class="block font-medium mb-1">Gender</label>
          <select v-model="form.gender" class="w-full p-2 border rounded">
            <option value="">Select Gender</option>
            <option value="male">Male</option>
            <option value="female">Female</option>
            <option value="other">Other</option>
          </select>
        </div>

        <div>
          <label class="block font-medium mb-1">Date of Birth</label>
          <input type="date" v-model="form.dateOfBirth" class="w-full p-2 border rounded"/>
        </div>

        <div>
          <label class="block font-medium mb-1">New Password (optional)</label>
          <input type="password" v-model="form.password" class="w-full p-2 border rounded"/>
        </div>
        <div>
          <label class="block font-medium mb-1">Confirm New Password</label>
          <input type="password" v-model="form.passwordConfirmation" class="w-full p-2 border rounded"/>
        </div>

        <div class="flex justify-end mt-6">
          <button
              type="submit"
              :disabled="saving"
              class="bg-brown-600 text-white px-4 py-2 rounded hover:bg-brown-700 disabled:opacity-50"
          >
            {{ saving ? 'Updating…' : 'Update Profile' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { reactive, ref, watch } from 'vue'
import axios from 'axios'
import Toast from '@/components/Toast.vue'

// Accept the existing user object as a prop
const props = defineProps({
  user: {
    type: Object,
    required: true
  }
})
const emit = defineEmits(['close', 'updated'])

// API config
const api   = process.env.VUE_APP_API_URL || 'https://localhost:5001'
const token = localStorage.getItem('authToken')

// Toast management
const toasts = reactive([])
function toast(message, type = 'success') {
  const id = Date.now() + Math.random()
  toasts.push({ id, message, type })
  setTimeout(() => removeToast(id), 4000)
}
function removeToast(id) {
  const idx = toasts.findIndex(t => t.id === id)
  if (idx !== -1) toasts.splice(idx, 1)
}

// Form definition
const fields = [
  { name: 'name',     label: 'Name',     type: 'text'  },
  { name: 'username', label: 'Username', type: 'text'  },
  { name: 'email',    label: 'Email',    type: 'email' },
  { name: 'phone',    label: 'Phone',    type: 'text'  }
]

const form = reactive({
  name: '',
  username: '',
  email: '',
  phone: '',
  gender: '',
  dateOfBirth: '',
  password: '',
  passwordConfirmation: ''
})

// Populate form when `user` prop changes, guard undefined
watch(
    () => props.user,
    user => {
      if (!user) return
      form.name                 = user.name || ''
      form.username             = user.username || ''
      form.email                = user.email || ''
      form.phone                = user.phone || ''
      form.gender               = user.gender || ''
      form.dateOfBirth          = user.dateOfBirth || ''
      form.password             = ''
      form.passwordConfirmation = ''
    },
    { immediate: true }
)

const saving = ref(false)

async function save() {
  saving.value = true
  try {
    const payload = {
      Name:                 form.name,
      Username:             form.username,
      Email:                form.email,
      Phone:                form.phone,
      Gender:               form.gender,
      DateOfBirth:          form.dateOfBirth || null,
      Password:             form.password || undefined,
      PasswordConfirmation: form.passwordConfirmation || undefined
    }

    const res = await axios.put(
        `${api}/api/account/profile`,
        payload,
        { headers: { Authorization: `Bearer ${token}` } }
    )

    toast(res.data.message)
    // Emit updated user back to parent
    emit('updated', res.data.user)
  } catch (err) {
    toast(err.response?.data?.message || 'Update failed', 'error')
  } finally {
    saving.value = false
  }
}
</script>
